#include <RoboCatPCH.h>

//zoom hardcoded at 100...if we want to lock players on screen, this could be calculated from zoom
bool isSquareMap = false;
const float HALF_WORLD_HEIGHT = 10.0f;
const float HALF_WORLD_WIDTH = 10.0f;

bool isCircleMap = true;
const float HALF_WORLD_CIRCLE_RADIUS = 13.0f;

RoboCat::RoboCat() :
	GameObject(),
	mMaxRotationSpeed( 5.f ),
	mMaxLinearSpeed( 2.f ),
	mVelocity( Vector3::Zero ),
	mWallRestitution( 0.1f ),
	mCatRestitution( 0.1f ),
	mWorldXDir(0.f),
	mWorldZDir(0.f),
	mPlayerId( 0 ),
	mIsShooting( false ),
	mHealth( 10 )
{
	SetCollisionRadius( 0.5f );
}

void RoboCat::ProcessInput( float inDeltaTime, const InputState& inInputState )
{
	//process our input....

	//turning...
	SetRotation(inInputState.GetDesiredRotationDelta());

	// moving...
	mWorldXDir = inInputState.GetDesiredHorizontalDelta();
	mWorldZDir = inInputState.GetDesiredVerticalDelta();

	mIsShooting = inInputState.IsShooting();
	mIsSwitchColor = inInputState.IsSwitchColor();

}

void RoboCat::AdjustVelocityByThrust( float inDeltaTime )
{
	//just set the velocity based on the thrust direction -- no thrust will lead to 0 velocity
	//simulating acceleration makes the client prediction a bit more complex

	// What I replaced is here
	mVelocity = Vector3(mWorldXDir, 0, mWorldZDir) * mMaxLinearSpeed;
}

void RoboCat::SimulateMovement( float inDeltaTime )
{
	//simulate us...
	AdjustVelocityByThrust( inDeltaTime );

	SetLocation( GetLocation() + mVelocity * inDeltaTime );

	ProcessCollisions();
}

void RoboCat::Update()
{
	
}

void RoboCat::ProcessCollisions()
{
	//right now just bounce off the sides..
	ProcessCollisionsWithScreenWalls();

	float sourceRadius = GetCollisionRadius();
	Vector3 sourceLocation = GetLocation();

	//now let's iterate through the world and see what we hit...
	//note: since there's a small number of objects in our game, this is fine.
	//but in a real game, brute-force checking collisions against every other object is not efficient.
	//it would be preferable to use a quad tree or some other structure to minimize the
	//number of collisions that need to be tested.
	for( auto goIt = World::sInstance->GetGameObjects().begin(), end = World::sInstance->GetGameObjects().end(); goIt != end; ++goIt )
	{
		GameObject* target = goIt->get();
		if (target->GetClassId() == 'BULT')
		{
			continue;
		}
		if (target != this && !target->DoesWantToDie())
		{
			//simple collision test for spheres- are the radii summed less than the distance?
			Vector3 targetLocation = target->GetLocation();
			float targetRadius = target->GetCollisionRadius();

			Vector3 delta = targetLocation - sourceLocation;
			float distSq = delta.LengthSq();
			float collisionDist = (sourceRadius + targetRadius);
			if (distSq < (collisionDist * collisionDist))
			{
				//first, tell the other guy there was a collision with a cat, so it can do something...

				if (target->HandleCollisionWithCat(this))
				{
					//okay, you hit something!
					//so, project your location far enough that you're not colliding
					Vector3 dirToTarget = delta;
					dirToTarget.Normalize();
					Vector3 acceptableDeltaFromSourceToTarget = dirToTarget * collisionDist;
					//important note- we only move this cat. the other cat can take care of moving itself
					SetLocation(targetLocation - acceptableDeltaFromSourceToTarget);


					Vector3 relVel = mVelocity;

					//if other object is a cat, it might have velocity, so there might be relative velocity...
					RoboCat* targetPlayer = target->GetAsCat();
					if (targetPlayer)
					{
						relVel -= targetPlayer->mVelocity;
					}

					//got vel with dir between objects to figure out if they're moving towards each other
					//and if so, the magnitude of the impulse ( since they're both just balls )
					float relVelDotDir = Dot(relVel, dirToTarget);

					if (relVelDotDir > 0.f)
					{
						Vector3 impulse = relVelDotDir * dirToTarget;

						if (targetPlayer)
						{
							mVelocity -= impulse;
							mVelocity *= mCatRestitution;
						}
						else
						{
							mVelocity -= impulse * 2.f;
							mVelocity *= mWallRestitution;
						}

					}
				}
			}
		}
	}

}

void RoboCat::ProcessCollisionsWithScreenWalls()
{
	Vector3 location = GetLocation();
	float x = location.mX;
	float y = location.mY;
	float z = location.mZ;

	float vx = mVelocity.mX;
	float vy = mVelocity.mY;
	float vz = mVelocity.mZ;

	float radius = GetCollisionRadius();

	//if the cat collides against a wall, the quick solution is to push it off
	if (isSquareMap) 
	{
		if ((z + radius) >= HALF_WORLD_HEIGHT && vz > 0)
		{
			mVelocity.mZ = -vz * mWallRestitution;
			location.mZ = HALF_WORLD_HEIGHT - radius;
			SetLocation(location);
		}
		else if (z <= (-HALF_WORLD_HEIGHT - radius) && vz < 0)
		{
			mVelocity.mZ = -vz * mWallRestitution;
			location.mZ = -HALF_WORLD_HEIGHT - radius;
			SetLocation(location);
		}

		if ((x + radius) >= HALF_WORLD_WIDTH && vx > 0)
		{
			mVelocity.mX = -vx * mWallRestitution;
			location.mX = HALF_WORLD_WIDTH - radius;
			SetLocation(location);
		}
		else if (x <= (-HALF_WORLD_WIDTH - radius) && vx < 0)
		{
			mVelocity.mX = -vx * mWallRestitution;
			location.mX = -HALF_WORLD_WIDTH - radius;
			SetLocation(location);
		}
	}
	else if (isCircleMap)
	{
		float xz = Vector3(x, 0, z).Length();
		if ((xz + radius) > HALF_WORLD_CIRCLE_RADIUS)
		{
			mVelocity.mX = -vx * HALF_WORLD_CIRCLE_RADIUS;
			mVelocity.mZ = -vz * HALF_WORLD_CIRCLE_RADIUS;
			location.mX = x / xz * (HALF_WORLD_CIRCLE_RADIUS - radius);
			location.mZ = z / xz * (HALF_WORLD_CIRCLE_RADIUS - radius);
			SetLocation(location);
		}
	}
}

uint32_t RoboCat::Write( OutputMemoryBitStream& inOutputStream, uint32_t inDirtyState ) const
{
	uint32_t writtenState = 0;

	if( inDirtyState & ECRS_PlayerId )
	{
		inOutputStream.Write( (bool)true );
		inOutputStream.Write( GetPlayerId() );

		writtenState |= ECRS_PlayerId;
	}
	else
	{
		inOutputStream.Write( (bool)false );
	}


	if( inDirtyState & ECRS_Pose )
	{
		inOutputStream.Write( (bool)true );

		Vector3 velocity = mVelocity;
		inOutputStream.Write( velocity.mX );
		//inOutputStream.Write( velocity.mY );
		inOutputStream.Write( velocity.mZ );

		Vector3 location = GetLocation();
		inOutputStream.Write( location.mX );
		//inOutputStream.Write( location.mY );
		inOutputStream.Write( location.mZ );

		inOutputStream.Write( GetRotation() );

		writtenState |= ECRS_Pose;
	}
	else
	{
		inOutputStream.Write( (bool)false );
	}

	//always write worldDir, can reduce size here
	if(mWorldXDir != 0.f || mWorldZDir != 0.f)
	{
		inOutputStream.Write( true );
		inOutputStream.Write(mWorldXDir);
		inOutputStream.Write(mWorldZDir);
	}
	else
	{
		inOutputStream.Write( false );
	}

	if( inDirtyState & ECRS_Color )
	{
		inOutputStream.Write( (bool)true );
		inOutputStream.Write( GetColor() );

		writtenState |= ECRS_Color;
	}
	else
	{
		inOutputStream.Write( (bool)false );
	}

	if( inDirtyState & ECRS_Health )
	{
		inOutputStream.Write( (bool)true );
		inOutputStream.Write( mHealth, 4 );

		writtenState |= ECRS_Health;
	}
	else
	{
		inOutputStream.Write( (bool)false );
	}
	

	return writtenState;
	

}


