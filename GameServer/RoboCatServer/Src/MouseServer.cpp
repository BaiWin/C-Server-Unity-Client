#include <RoboCatServerPCH.h>


MouseServer::MouseServer()
{
}

void MouseServer::HandleDying()
{
	NetworkManagerServer::sInstance->UnregisterGameObject( this );
}


bool MouseServer::HandleCollisionWithCat( RoboCat* inCat )
{
	//kill yourself!
	SetDoesWantToDie( true );

	ScoreBoardManager::sInstance->IncScore( inCat->GetPlayerId(), 1 );

	return false;
}

void MouseServer::Update()
{

	float deltaTime = Timing::sInstance.GetDeltaTime();

	const auto& gameObjects = World::sInstance->GetGameObjects();

	Vector3 dir = Vector3();
	for (GameObjectPtr gameObject : gameObjects)
	{
		float compDist = INFINITY;
		if (gameObject->GetClassId() == 'PLER')
		{
			float dist = (GetLocation() - gameObject->GetLocation()).Length();
			if (dist < compDist)
			{
				compDist = dist;
				dir = gameObject->GetLocation() - GetLocation();
			}
		}
	}
	dir.Normalize();
	SetLocation(GetLocation() + dir * 0.5f * deltaTime);

	if (dir.Length() != 0 && !DoesWantToDie())
	{
		NetworkManagerServer::sInstance->SetStateDirty(GetNetworkId(), EMRS_Pose);
	}

	//we'll let the cats handle the collisions
	ProcessCollisions();
}

void MouseServer::ProcessCollisions()
{
	float sourceRadius = GetCollisionRadius();
	Vector3 sourceLocation = GetLocation();

	//now let's iterate through the world and see what we hit...
	//note: since there's a small number of objects in our game, this is fine.
	//but in a real game, brute-force checking collisions against every other object is not efficient.
	//it would be preferable to use a quad tree or some other structure to minimize the
	//number of collisions that need to be tested.
	for (auto goIt = World::sInstance->GetGameObjects().begin(), end = World::sInstance->GetGameObjects().end(); goIt != end; ++goIt)
	{
		GameObject* target = goIt->get();
		if (target->GetClassId() != 'BULT')
		{
			continue;
		}
		if (target->GetColor() != GetColor())
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

				if (target->HandleCollisionWithOther(this))
				{
					//okay, you hit something!
					//so, project your location far enough that you're not colliding
					Vector3 dirToTarget = delta;
					dirToTarget.Normalize2D();
					Vector3 acceptableDeltaFromSourceToTarget = dirToTarget * collisionDist;
					//important note- we only move this cat. the other cat can take care of moving itself
					SetLocation(targetLocation - acceptableDeltaFromSourceToTarget);
				}
			}
		}
	}

}


