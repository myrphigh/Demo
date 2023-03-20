using System;
using UnityEngine;
using GameCreator.Runtime.Common;

namespace GameCreator.Runtime.Characters
{
    [Title("Rigidbody")]
    [Image(typeof(IconPhysics), ColorTheme.Type.Yellow)]
    
    [Category("Rigidbody")]
    [Description("Moves the Character using a physics based Rigidbody component")]
    
    [Serializable]
    public class UnitDriverRigidbody : TUnitDriver
    {
        protected enum Plane
        {
            None,
            XY,
            XZ,
            YZ,
        }
        
        // EXPOSED MEMBERS: -----------------------------------------------------------------------
        
        [SerializeField] protected PhysicMaterial m_Material;

        [SerializeField]
        private RigidbodyInterpolation m_Interpolation = RigidbodyInterpolation.Interpolate;
        
        [SerializeField] protected float m_GroundDistance = 0.1f;
        [SerializeField] protected LayerMask m_GroundMask = -1;

        [SerializeField] protected Plane m_Plane = Plane.None;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] protected CapsuleCollider m_Capsule;
        [NonSerialized] protected Rigidbody m_Rigidbody;
        
        [NonSerialized] private RaycastHit[] m_HitsBuffer = new RaycastHit[1];
         
        [NonSerialized] protected float m_LastVerticalSpeed;
        
        [NonSerialized] protected bool m_IsGrounded;
        [NonSerialized] protected AnimFloat m_IsGroundedSmooth;
        [NonSerialized] protected AnimVector3 m_FloorNormal;
        
        [NonSerialized] protected int m_GroundFrame = -100;
        [NonSerialized] protected float m_GroundTime = -100f;
        [NonSerialized] protected float m_JumpTime = -100f;
        
        // INTERFACE PROPERTIES: ------------------------------------------------------------------

        public override Vector3 WorldMoveDirection => this.m_Rigidbody.velocity;
        public override Vector3 LocalMoveDirection => this.Transform.InverseTransformDirection(
            this.WorldMoveDirection
        );

        public override float SkinWidth => 0f;
        public override bool IsGrounded => this.m_IsGrounded;
        public override Vector3 FloorNormal => this.m_FloorNormal.Current;
        
        public override bool Collision
        {
            get => this.m_Capsule.enabled;
            set => this.m_Capsule.enabled = value;
        }

        // INITIALIZERS: --------------------------------------------------------------------------

        public UnitDriverRigidbody()
        {
            this.m_LastVerticalSpeed = 0f;
        }

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);

            this.m_IsGroundedSmooth = new AnimFloat(1f, 0.01f);
            this.m_FloorNormal = new AnimVector3(Vector3.up, 0.05f);

            this.m_Capsule = this.Character.GetComponent<CapsuleCollider>();
            if (!this.m_Capsule)
            {
                GameObject instance = this.Character.gameObject;
                this.m_Capsule = instance.AddComponent<CapsuleCollider>();
                this.m_Capsule.hideFlags = HideFlags.HideInInspector;
            }
            
            this.m_Rigidbody = this.Character.GetComponent<Rigidbody>();
            if (!this.m_Rigidbody)
            {
                GameObject instance = this.Character.gameObject;
                this.m_Rigidbody = instance.AddComponent<Rigidbody>();
                this.m_Rigidbody.hideFlags = HideFlags.HideInInspector;
            }

            character.Ragdoll.EventBeforeStartRagdoll += this.OnStartRagdoll;
            character.Ragdoll.EventAfterStartRecover += this.OnEndRagdoll;

            this.m_Rigidbody.useGravity = false;
            this.m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            this.m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public override void OnDispose(Character character)
        {
            base.OnDispose(character);

            UnityEngine.Object.Destroy(this.m_Capsule);
            UnityEngine.Object.Destroy(this.m_Rigidbody);
            
            character.Ragdoll.EventBeforeStartRagdoll -= this.OnStartRagdoll;
            character.Ragdoll.EventAfterStartRecover -= this.OnEndRagdoll;
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        public override void OnUpdate()
        {
            if (this.Character.IsDead) return;
            
            this.UpdateProperties();
            this.UpdateJump(this.Character.Motion);
        }

        public override void OnFixedUpdate()
        {
            if (this.Character.IsDead) return;
            
            this.CheckGround(this.Character.Motion);
            this.UpdateGravity(this.Character.Motion);
            
            this.UpdateTranslation(this.Character.Motion);
        }

        protected virtual void UpdateProperties()
        {
            this.m_FloorNormal.UpdateWithDelta(this.Character.Time.DeltaTime);
            
            float height = this.Character.Motion.Height;
            float radius = this.Character.Motion.Radius;

            if (Math.Abs(this.m_Capsule.height - height) > float.Epsilon)
            {
                this.m_Capsule.height = height;   
            }
            
            if (Math.Abs(this.m_Capsule.radius - radius) < float.Epsilon)
            {
                this.m_Capsule.radius = radius;
            }
            
            if (this.m_Capsule.center != Vector3.zero)
            {
                this.m_Capsule.center = Vector3.zero;
            }
            
            if (this.m_Material != null)
            {
                this.m_Capsule.material = this.m_Material;
            }
            
            if (this.m_Rigidbody.interpolation != this.m_Interpolation)
            {
                this.m_Rigidbody.interpolation = this.m_Interpolation;
            }
            
            if (Math.Abs(this.m_Rigidbody.mass - this.Character.Motion.Mass) > float.Epsilon)
            {
                this.m_Rigidbody.mass = this.Character.Motion.Mass;
            }
        }

        protected virtual void CheckGround(IUnitMotion motion)
        {
            int hitCount = Physics.RaycastNonAlloc(
                this.Character.Feet + Vector3.up * this.m_GroundDistance, 
                Vector3.down,
                this.m_HitsBuffer,
                this.m_GroundDistance * 2f,
                this.m_GroundMask,
                QueryTriggerInteraction.Ignore
            );

            this.m_IsGrounded = hitCount > 0;
            this.m_FloorNormal.Target = this.m_IsGrounded
                ? this.m_HitsBuffer[0].normal
                : Vector3.up;

            float deltaTime = this.Character.Time.FixedDeltaTime;
            this.m_IsGroundedSmooth.UpdateWithDelta(
                this.m_IsGrounded ? 1f : 0f, 
                COYOTE_TIME, 
                deltaTime
            );
        }

        protected virtual void UpdateJump(IUnitMotion motion)
        {
            if (!motion.IsJumping) return;
            if (!motion.CanJump) return;
            
            bool jumpCooldown = this.m_JumpTime + motion.JumpCooldown < this.Character.Time.Time;
            if (!jumpCooldown) return;

            this.m_Rigidbody.AddForce(
                Vector3.up * motion.IsJumpingForce, 
                ForceMode.VelocityChange
            );
            
            this.m_JumpTime = this.Character.Time.Time;
            this.Character.OnJump(motion.IsJumpingForce);
        }

        protected virtual void UpdateGravity(IUnitMotion motion)
        {
            Vector3 mass = Vector3.up * this.m_Rigidbody.mass;
            float gravity = this.WorldMoveDirection.y >= 0f 
                ? motion.GravityUpwards 
                : motion.GravityDownwards;
            
            this.m_Rigidbody.AddForce(
                mass * gravity, 
                ForceMode.Force
            );

            if (this.m_IsGrounded)
            {
                if (this.Character.Time.Time - this.m_GroundTime > COYOTE_TIME &&
                    this.Character.Time.Frame - this.m_GroundFrame > COYOTE_FRAMES)
                {
                    this.Character.OnLand(this.m_LastVerticalSpeed);
                }
                
                this.m_GroundTime = this.Character.Time.Time;
                this.m_GroundFrame = this.Character.Time.Frame;
            }

            Vector3 velocity = this.m_Rigidbody.velocity;
            this.m_Rigidbody.velocity = new Vector3(
                velocity.x,
                Mathf.Max(velocity.y, motion.TerminalVelocity),
                velocity.z
            );

            this.m_LastVerticalSpeed = this.m_Rigidbody.velocity.y;
        }

        protected virtual void UpdateTranslation(IUnitMotion motion)
        {
            Vector3 kinetic = motion.MovementType switch
            {
                Character.MovementType.MoveToDirection => this.UpdateMoveToDirection(motion),
                Character.MovementType.MoveToPosition => this.UpdateMoveToPosition(motion),
                _ => Vector3.zero
            };

            Vector3 rootMotion = this.Character.Animim.RootMotionDeltaPosition;
            Vector3 movement = Vector3.Lerp(kinetic, rootMotion, this.Character.RootMotionPosition);

            Vector3 horizontalVelocity = Vector3.Scale(
                this.m_Rigidbody.velocity,
                Vector3Plane.NormalUp
            );
            
            bool isSlowing = movement.magnitude <= horizontalVelocity.magnitude;
            this.m_Rigidbody.drag = isSlowing && this.Character.Motion.UseAcceleration
                ? this.Character.Motion.Deceleration
                : 0f;

            movement = movement.normalized * (this.Character.Motion.UseAcceleration
                ? this.Character.Motion.Acceleration
                : 9999f);

            if (horizontalVelocity.magnitude >= this.Character.Motion.LinearSpeed)
            {
                if (Mathf.Abs(horizontalVelocity.x + movement.x) >= Mathf.Abs(horizontalVelocity.x))
                {
                    movement.x = 0f;
                }
                
                if (Mathf.Abs(horizontalVelocity.z + movement.z) >= Mathf.Abs(horizontalVelocity.z))
                {
                    movement.z = 0f;
                }
            }
            
            this.m_Rigidbody.AddForce(
                new Vector3(movement.x, this.m_Rigidbody.velocity.y, movement.z),
                ForceMode.Acceleration
            );
        }
        
        protected virtual void UpdateLockPlane(IUnitMotion motion)
        {
            if (this.m_Plane == Plane.None) return;
            Vector3 position = this.Transform.position;
            
            this.SetPosition(this.m_Plane switch
            {
                Plane.XY => new Vector3(position.x, position.y, 0f),
                Plane.XZ => new Vector3(position.x, 0f, position.z),
                Plane.YZ => new Vector3(0f, position.y, position.z),
                Plane.None => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            });
        }

        // POSITION METHODS: ----------------------------------------------------------------------

        protected virtual Vector3 UpdateMoveToDirection(IUnitMotion motion)
        {
            return motion.MoveDirection;
        }

        protected virtual Vector3 UpdateMoveToPosition(IUnitMotion motion)
        {
            float distance = Vector3.Distance(this.Character.Feet, motion.MovePosition);
            float brakeRadiusHeuristic = Math.Max(motion.Height, motion.Radius * 2f);
            float velocity = motion.MoveDirection.magnitude;
            
            if (distance < brakeRadiusHeuristic)
            {
                velocity = Mathf.Lerp(
                    motion.LinearSpeed, motion.LinearSpeed * 0.25f,
                    1f - Mathf.Clamp01(distance / brakeRadiusHeuristic)
                );
            }
            
            return motion.MoveDirection.normalized * velocity;
        }

        // INTERFACE METHODS: ---------------------------------------------------------------------

        public override void SetPosition(Vector3 position)
        {
            position += Vector3.up * (this.Character.Motion.Height * 0.5f);
            this.Transform.position = position;
            Physics.SyncTransforms();
        }

        public override void SetRotation(Quaternion rotation)
        {
            this.Transform.rotation = rotation;
            Physics.SyncTransforms();
        }

        public override void SetScale(Vector3 scale)
        {
            this.Transform.localScale = scale;
            Physics.SyncTransforms();
        }

        public override void AddPosition(Vector3 amount)
        {
            this.Transform.position += amount;
            Physics.SyncTransforms();
        }

        public override void AddRotation(Quaternion amount)
        {
            this.Transform.rotation *= amount;
            Physics.SyncTransforms();
        }
        
        public override void AddScale(Vector3 scale)
        {
            this.Transform.localScale += scale;
            Physics.SyncTransforms();
        }

        // CALLBACK METHODS: ----------------------------------------------------------------------
        
        private void OnStartRagdoll()
        {
            this.m_Rigidbody.isKinematic = true;
            this.m_Capsule.enabled = false;
        }
        
        private void OnEndRagdoll()
        {
            this.m_Capsule.enabled = true;
            this.m_Rigidbody.isKinematic = false;
            this.m_Rigidbody.velocity = Vector3.zero;
        }

        // GIZMOS: --------------------------------------------------------------------------------

        public override void OnDrawGizmos(Character character)
        {
            if (!Application.isPlaying) return;

            IUnitMotion motion = character.Motion;
            if (motion == null) return;

            switch (motion.MovementType)
            {
                case Character.MovementType.MoveToPosition:
                    this.OnDrawGizmosToTarget(motion);
                    break;
            }
        }

        protected void OnDrawGizmosToTarget(IUnitMotion motion)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(this.Character.Feet, motion.MovePosition);
        }
        
        // STRING: --------------------------------------------------------------------------------

        public override string ToString() => "Rigidbody";
    }
}