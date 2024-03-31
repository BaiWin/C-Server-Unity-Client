
class InputState
{
public:

	InputState() :
	mHorizontal(0),
	mVertical(0),
	mRotation(0),
	mIsShooting(false),
	mIsSwitchColor(false),
	mColor(0)
	{}

	float GetDesiredHorizontalDelta()	const { return mHorizontal; }
	float GetDesiredVerticalDelta()		const { return mVertical; }
	float GetDesiredRotationDelta()		const { return mRotation; }
	bool  IsShooting()					const { return mIsShooting; }
	bool  IsSwitchColor()               const { return mIsSwitchColor; }
	uint8_t   GetColor()				const { return mColor; }

	bool Write( OutputMemoryBitStream& inOutputStream ) const;
	bool Read( InputMemoryBitStream& inInputStream );

private:
	friend class InputManager;

	float	mHorizontal;
	float	mVertical;
	float   mRotation;
	bool	mIsShooting;
	bool    mIsSwitchColor;
	uint8_t     mColor;
};