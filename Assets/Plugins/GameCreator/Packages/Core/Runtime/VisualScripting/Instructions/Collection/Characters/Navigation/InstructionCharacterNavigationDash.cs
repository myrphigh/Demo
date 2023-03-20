using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Title("Dash")]
    [Description("Moves the Character in the chosen direction for a brief period of time")]

    [Category("Characters/Navigation/Dash")]

    [Parameter("Direction", "Vector oriented towards the desired direction")]
    [Parameter("Speed", "Velocity the Character moves throughout the whole movement")]
    [Parameter("Damping", "Defines the duration and gradually changes the rate of the movement over time")]
    [Parameter("Wait to Finish", "If true this Instruction waits until the dash is completed")]
    
    [Parameter("Animation Forward", "Animation played on the Character when dashing forward")]
    [Parameter("Animation Backward", "Animation played on the Character when dashing backwards")]
    [Parameter("Animation Right", "Animation played on the Character when dashing right")]
    [Parameter("Animation Left", "Animation played on the Character when dashing left")]

    [Example(
        "The Damping value defines both the duration and the velocity rate at which the Character " +
        "moves when performing the Dash. To change the duration of the dash open the animation " +
        "curve window and move the last keyframe to the left to decrease the duration or to the " +
        "right to increase it."
    )]
    
    [Example(
        "The Damping value also defines the coefficient rate at which the Character moves " +
        "while performing the Dash. By default the Character starts with a coefficient of 0. " +
        "After 0.2 seconds it increases to 1 and goes back to 0 after 0.8 seconds. This curve " +
        "is evaluated while performing a Dash and the coefficient is extracted from the curve " +
        "and multiplied by the Speed to gradually change the rate at which the Character moves. " +
        "For this reason, it is recommended that the Damping stay between 0 and 1."
    )]
    
    [Keywords("Leap", "Blink", "Roll", "Flash")]
    [Image(typeof(IconCharacterDash), ColorTheme.Type.Blue)]

    [Serializable]
    public class InstructionCharacterNavigationDash : TInstructionCharacterNavigation
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField] private PropertyGetDirection m_Direction = GetDirectionCharactersMoving.Create;
        [SerializeField] private PropertyGetDecimal m_Velocity = new PropertyGetDecimal(10f);
        
        [SerializeField] private AnimationCurve m_Damping = new AnimationCurve(
            new Keyframe(0.0f, 0.0f, 0f, 0f), 
            new Keyframe(0.1f, 1.0f, 0f, 0f),
            new Keyframe(0.5f, 0.5f, 0f, 0f)
        );

        [SerializeField] private bool m_WaitToFinish = true;

        [SerializeField] private AnimationClip m_AnimationForward;
        [SerializeField] private AnimationClip m_AnimationBackward;
        [SerializeField] private AnimationClip m_AnimationRight;
        [SerializeField] private AnimationClip m_AnimationLeft;
        
        [SerializeField] private float m_Speed = 1f;
        [SerializeField] private float m_TransitionIn = 0.1f;
        [SerializeField] private float m_TransitionOut = 0.25f;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Dash {this.m_Character} towards {this.m_Direction}";

        // RUN METHOD: ----------------------------------------------------------------------------
        
        protected override async Task Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            if (character == null) return;
            if (character.Busy.AreLegsBusy) return;

            Vector3 direction = this.m_Direction.Get(args);
            if (direction == Vector3.zero) direction = character.transform.forward;
            
            float speed = (float) this.m_Velocity.Get(args);
            
            if (!character.Dash.CanDash()) return;
            Task task = character.Dash.Execute(direction, speed, this.m_Damping);

            character.Busy.MakeLegsBusy();
            float angle = Vector3.SignedAngle(
                direction, 
                character.transform.forward, 
                Vector3.up
            );
            
            AnimationClip animationClip = this.GetAnimationClip(angle);
            if (animationClip != null)
            {
                ConfigGesture config = new ConfigGesture(
                    0f, animationClip.length, this.m_Speed, false,
                    this.m_TransitionIn, this.m_TransitionOut
                );
                
                _ = character.Gestures.CrossFade(animationClip, null, BlendMode.Blend, config, true);
            }

            if (this.m_WaitToFinish) await task;
        }

        private AnimationClip GetAnimationClip(float angle)
        {
            return angle switch
            {
                <= 45f and >= -45f => this.m_AnimationForward,
                < 135f and > 45f => this.m_AnimationRight,
                > -135f and < -45f => this.m_AnimationLeft,
                _ => m_AnimationBackward
            };
        }
    }
}