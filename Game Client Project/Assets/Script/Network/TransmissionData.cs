using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransmissionData
{
    public abstract void HandleDeliveryFailure(DeliveryNotificationManager inDeliveryNotificationManager);
    public abstract void HandleDeliverySuccess(DeliveryNotificationManager inDeliveryNotificationManager);
}
