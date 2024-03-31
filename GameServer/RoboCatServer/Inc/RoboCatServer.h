enum ECatControlType
{
	ESCT_Human,
	ESCT_AI
};

class RoboCatServer : public RoboCat
{
public:
	static GameObjectPtr	StaticCreate() { return NetworkManagerServer::sInstance->RegisterAndReturn( new RoboCatServer() ); }
	virtual void HandleDying() override;

	virtual void Update();

	void SetCatControlType( ECatControlType inCatControlType ) { mCatControlType = inCatControlType; }

	void TakeDamage( int inDamagingPlayerId );

protected:
	RoboCatServer();

private:

	void HandleShooting();

	void HandleSwitchingColor();

	ECatControlType	mCatControlType;


	float		mTimeOfNextShot;
	float		mTimeBetweenShots;

};