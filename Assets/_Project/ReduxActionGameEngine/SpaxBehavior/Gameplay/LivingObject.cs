using ActionGameEngine.Data;
using Spax;
using BEPUutilities;
using BEPUUnity;

namespace ActionGameEngine
{
    //object that can move around and detect physics
    public abstract class LivingObject : SpaxBehavior
    {

        //overall data about our character, stuff like all states and movelist
        protected CharacterData data;
        //rigidbody that we will use to move around and collide with the environment
        protected ShapeBase rb;

        //current status about the character, state, persistent state conditions, current hp, etc.
        protected CharacterStatus status;

        //hitstop timer
        protected FrameTimer stopTimer;
        //for keeping track of out state
        protected FrameTimer stateTimer;
        //velocity calculated that we will apply to our rigidbody
        protected BepuVector3 calcVel;
        //for things such as setting velocity, makes sure that that velocity is always being applied
        protected BepuVector4 storedVel;

    }
}
