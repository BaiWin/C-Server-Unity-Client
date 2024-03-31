using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryNotificationManager
{
    const float kDelayBeforeAckTimeout = 0.5f;

    uint mNextOutgoingSequenceNumber;
    uint mNextExpectedSequenceNumber;

    List<InFlightPacket> mInFlightPackets = new List<InFlightPacket>();
    List<AckRange> mPendingAcks = new List<AckRange>();

    bool mShouldSendAcks;
    bool mShouldProcessAcks;

    uint mDeliveredPacketCount;
    uint mDroppedPacketCount;
    uint mDispatchedPacketCount;

    public DeliveryNotificationManager(bool mShouldSendAcks, bool mShouldProcessAcks)
    {
        this.mShouldSendAcks = mShouldSendAcks;
        this.mShouldProcessAcks = mShouldProcessAcks;
    }

    public InFlightPacket WriteState(OutputMemoryBitStream inOutputStream)
    {
        InFlightPacket toRet = WriteSequenceNumber(inOutputStream);
        if (mShouldSendAcks)
        {
            WriteAckData(inOutputStream);
        }
        return toRet;
    }

    public bool ReadAndProcessState(InputMemoryBitStream inInputStream)
    {
        bool toRet = ProcessSequenceNumber(inInputStream);
        if (mShouldProcessAcks) // false
        {
            ProcessAcks(inInputStream);
        }
        return toRet;
    }

    void ProcessTimedOutPackets()
    {
        float timeoutTime = Time.time - kDelayBeforeAckTimeout;

        while (mInFlightPackets.Count > 0)
        {
            var nextInFlightPacket = mInFlightPackets[0];

            //was this packet dispatched before the current time minus the timeout duration?
            if (nextInFlightPacket.GetTimeDispatched() < timeoutTime)
            {
                //it failed! let us know about that
                HandlePacketDeliveryFailure(nextInFlightPacket);
                mInFlightPackets.RemoveAt(0);
            }
            else
            {
                //it wasn't, and packets are all in order by time here, so we know we don't have to check farther
                break;
            }
        }
    }

    uint GetDroppedPacketCount() { return mDroppedPacketCount; }
    uint GetDeliveredPacketCount() { return mDeliveredPacketCount; }
	uint GetDispatchedPacketCount() { return mDispatchedPacketCount; }
	
	public List<InFlightPacket>	GetInFlightPackets() { return mInFlightPackets; }

    private InFlightPacket WriteSequenceNumber(OutputMemoryBitStream inOutputStream)
    {
        //write the sequence number, but also create an inflight packet for this...
        uint sequenceNumber = mNextOutgoingSequenceNumber++;
        inOutputStream.Write(sequenceNumber);

        ++mDispatchedPacketCount;

        if (mShouldProcessAcks)
        {
            mInFlightPackets.Add(new InFlightPacket(sequenceNumber));

            return mInFlightPackets[mInFlightPackets.Count - 1];
        }
        else
        {
            return null;
        }
    }

    private void WriteAckData(OutputMemoryBitStream inOutputStream)
    {
        //we usually will only have one packet to ack
        //so we'll follow that with a 0 bit if that's the case
        //however, if we have more than 1, we'll make that 1 bit a 1 and then write 8 bits of how many packets
        //we could do some statistical analysis to determine if this is the best strategy but we'll use it for now

        //do we have any pending acks?
        //if so, write a 1 bit and write the first range
        //otherwise, write 0 bit
        bool hasAcks = (mPendingAcks.Count > 0);

        inOutputStream.Write(hasAcks);
        if (hasAcks)
        {
            //note, we could write all the acks
            mPendingAcks[0].Write(inOutputStream);
            mPendingAcks.RemoveAt(0);
        }
    }

    //returns wether to drop the packet- if sequence number is too low!
    private bool ProcessSequenceNumber(InputMemoryBitStream inInputStream)
    {
        uint sequenceNumber;

        inInputStream.Read(out sequenceNumber);
        if (sequenceNumber == mNextExpectedSequenceNumber)
        {
            mNextExpectedSequenceNumber = sequenceNumber + 1;
            //is this what we expect? great, let's add an ack to our pending list
            if (mShouldSendAcks)
            {
                AddPendingAck(sequenceNumber);
            }
            //and let's continue processing this packet...
            return true;
        }
        //is the sequence number less than our current expected sequence? silently drop it.
        //if this is due to wrapping around, we might fail to ack some packets that we should ack, but they'll get resent, so it's not a big deal
        //note that we don't have to re-ack it because our system doesn't reuse sequence numbers
        else if (sequenceNumber < mNextExpectedSequenceNumber)
        {
            return false;
        }
        else if (sequenceNumber > mNextExpectedSequenceNumber)
        {
            //we missed a lot of packets!
            //so our next expected packet comes after this one...
            mNextExpectedSequenceNumber = sequenceNumber + 1;
            //we should nack the missing packets..this will happen automatically inside AddPendingAck because
            //we're adding an unconsequitive ack
            //and then we can ack this and process it
            if (mShouldSendAcks)
            {
                AddPendingAck(sequenceNumber);
            }
            return true;
        }

        //drop packet if we couldn't even read sequence number!
        return false;
    }

    //in each packet we can ack a range
    //anything in flight before the range will be considered nackd by the other side immediately
    private void ProcessAcks(InputMemoryBitStream inInputStream)
    {
        bool hasAcks;
        inInputStream.Read(out hasAcks);
        if (hasAcks)
        {
            AckRange ackRange = new AckRange();
            ackRange.Read(inInputStream);

            //for each InfilghtPacket with a sequence number less than the start, handle delivery failure...
            uint nextAckdSequenceNumber = ackRange.GetStart();
            uint onePastAckdSequenceNumber = nextAckdSequenceNumber + ackRange.GetCount();
            while (nextAckdSequenceNumber < onePastAckdSequenceNumber && mInFlightPackets.Count > 0)
            {
                var nextInFlightPacket = mInFlightPackets[0];
                //if the packet has a lower sequence number, we didn't get an ack for it, so it probably wasn't delivered
                uint nextInFlightPacketSequenceNumber = nextInFlightPacket.GetSequenceNumber();
                if (nextInFlightPacketSequenceNumber < nextAckdSequenceNumber)
                {
                    //copy this so we can remove it before handling the failure- we don't want to find it when checking for state
                    var copyOfInFlightPacket = nextInFlightPacket;
                    mInFlightPackets.RemoveAt(0);
                    HandlePacketDeliveryFailure(copyOfInFlightPacket);
                }
                else if (nextInFlightPacketSequenceNumber == nextAckdSequenceNumber)
                {
                    HandlePacketDeliverySuccess(nextInFlightPacket);
                    //received!
                    mInFlightPackets.RemoveAt(0);
                    //decrement count, advance nextAckdSequenceNumber
                    ++nextAckdSequenceNumber;
                }
                else if (nextInFlightPacketSequenceNumber > nextAckdSequenceNumber)
                {
                    //we've already ackd some packets in here.
                    //keep this packet in flight, but keep going through the ack...
                    ++nextAckdSequenceNumber;
                }
            }
        }
    }


    private void AddPendingAck(uint inSequenceNumber)
    {
        //if you don't have a range yet, or you can't correctly extend the final range with the sequence number,
        //start a new range
        if (mPendingAcks.Count == 0 || !mPendingAcks[mPendingAcks.Count - 1].ExtendIfShould(inSequenceNumber))
        {
            mPendingAcks.Add(new AckRange(inSequenceNumber));
        }
    }

    private void HandlePacketDeliveryFailure(InFlightPacket inFlightPacket)
    {
        ++mDroppedPacketCount;
        inFlightPacket.HandleDeliveryFailure(this);
    }

	private void HandlePacketDeliverySuccess(InFlightPacket inFlightPacket)
    {
        ++mDeliveredPacketCount;
        inFlightPacket.HandleDeliverySuccess(this);
    }
}
