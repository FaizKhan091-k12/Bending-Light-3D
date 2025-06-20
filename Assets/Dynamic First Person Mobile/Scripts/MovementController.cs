


using UnityEngine;
using FirstPersonMobileTools.Utility;

namespace FirstPersonMobileTools.DynamicFirstPerson
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CameraLook))]
    public class MovementController : MonoBehaviour
    {
        #region Class accessible field
        [HideInInspector] public bool Input_Sprint { get; set; }
        [HideInInspector] public bool Input_Jump { get; set; }
        [HideInInspector] public bool Input_Crouch { get; set; }

        [HideInInspector] public float Walk_Speed { private get { return m_WalkSpeed; } set { m_WalkSpeed = value; } }
        [HideInInspector] public float Run_Speed { private get { return m_RunSpeed; } set { m_RunSpeed = value; } }
        [HideInInspector] public float Crouch_Speed { private get { return m_CrouchSpeed; } set { m_CrouchSpeed = value; } }
        [HideInInspector] public float Jump_Force { private get { return m_JumpForce; } set { m_JumpForce = value; } }
        [HideInInspector] public float Acceleration { private get { return m_Acceleration; } set { m_Acceleration = value; } }
        [HideInInspector] public float Land_Momentum { private get { return m_LandMomentum; } set { m_LandMomentum = value; } }
        #endregion

        #region Editor accessible field
        [SerializeField] private Joystick m_Joystick;

        [SerializeField] private float m_Acceleration = 1.0f;
        [SerializeField] private float m_WalkSpeed = 1.0f;
        [SerializeField] private float m_RunSpeed = 3.0f;
        [SerializeField] private float m_CrouchSpeed = 0.5f;
        [SerializeField] private float m_CrouchDelay = 0.5f;
        [SerializeField] private float m_CrouchHeight = 1.0f;

        [SerializeField] private float m_JumpForce = 1.0f;
        [SerializeField] private float m_Gravity = 10.0f;
        [SerializeField] private float m_LandMomentum = 2.0f;

        [SerializeField] private AudioClip[] m_FootStepSounds;
        [SerializeField] private AudioClip m_JumpSound;
        [SerializeField] private AudioClip m_LandSound;

        [SerializeField] private Bobbing m_WalkBob = new Bobbing();
        [SerializeField] private Bobbing m_IdleBob = new Bobbing();
        #endregion

        private Camera m_Camera;
        private CharacterController m_CharacterController;
        private CameraLook m_CameraLook;
        private AudioSource m_AudioSource;

        private Vector3 m_MovementDirection;
        public Vector3 m_HeadMovement;
        private Vector3 m_LandBobRange_FinalImpact;
        private Vector3 m_OriginalScale;
        private const float m_StickToGround = -1f;
        private float m_MinFallLand = -10f;
        private float m_CrouchTimeElapse = 0.0f;
        private float m_OriginalLandMomentum;
        private bool m_IsFloating = false;

        private float m_MovementVelocity
        {
            get { return new Vector2(m_CharacterController.velocity.x, m_CharacterController.velocity.z).magnitude; }
        }

        private Vector2 Input_Movement
        {
            get { return m_Joystick != null ? new Vector2(m_Joystick.Horizontal, m_Joystick.Vertical) : Vector2.zero; }
        }

        private bool m_IsWalking
        {
            get { return m_MovementVelocity > 0.0f; }
        }

        private bool m_OnLanded
        {
            get { return m_IsFloating && m_MovementDirection.y < m_MinFallLand && m_CharacterController.isGrounded; }
        }

        private bool m_OnJump
        {
            get { return !m_CharacterController.isGrounded && !m_IsFloating; }
        }

        private float m_speed
        {
            get
            {
                bool hasMovementInput = Input_Movement.magnitude != 0 || External_Input_Movement.magnitude != 0;
                return Input_Crouch ? m_CrouchSpeed : Input_Sprint ? m_RunSpeed : hasMovementInput ? m_WalkSpeed : 0.0f;
            }
        }

#if UNITY_EDITOR
        public Vector2 External_Input_Movement;
#else
private Vector2 External_Input_Movement;
#endif
        private void Start()
        {
            m_Camera = GetComponentInChildren<Camera>();
            m_AudioSource = GetComponent<AudioSource>();
            m_CharacterController = GetComponent<CharacterController>();
            m_CameraLook = GetComponent<CameraLook>();

            m_CharacterController.height = m_CharacterController.height;
            m_OriginalScale = transform.localScale;
            m_OriginalLandMomentum = m_LandMomentum;

            m_WalkBob.SetUp();
            m_IdleBob.SetUp();
        }

        private void Update()
        {
            Handle_InputMovement();
            Handle_AirMovement();
            Handle_Crouch();
            Handle_Step();

            UpdateWalkBob();

            m_CharacterController.Move(m_MovementDirection * Time.deltaTime);

            m_Camera.transform.localPosition += m_HeadMovement;
            m_HeadMovement = Vector3.zero;
        }

        private void Handle_InputMovement()
        {
            Vector2 Input;
            Input.x = Input_Movement.x == 0 ? External_Input_Movement.x : Input_Movement.x;
            Input.y = Input_Movement.y == 0 ? External_Input_Movement.y : Input_Movement.y;

            Vector3 WalkTargetDirection =
                Input.y * transform.forward * m_speed +
                Input.x * transform.right * m_speed;

            WalkTargetDirection = Input_Sprint && WalkTargetDirection == Vector3.zero ? transform.forward * m_speed : WalkTargetDirection;

            m_MovementDirection.x = Mathf.MoveTowards(m_MovementDirection.x, WalkTargetDirection.x, m_Acceleration * Time.deltaTime);
            m_MovementDirection.z = Mathf.MoveTowards(m_MovementDirection.z, WalkTargetDirection.z, m_Acceleration * Time.deltaTime);

            if (m_LandMomentum != m_OriginalLandMomentum)
            {
                m_LandMomentum = Mathf.Clamp(m_LandMomentum + Time.deltaTime, 0, m_OriginalLandMomentum);
                m_MovementDirection.x *= m_LandMomentum / m_OriginalLandMomentum;
                m_MovementDirection.z *= m_LandMomentum / m_OriginalLandMomentum;
            }
        }

        private void Handle_AirMovement()
        {
            if (m_OnLanded)
            {
                m_LandMomentum = 0;
                PlaySound(m_LandSound);
            }

            if (m_CharacterController.isGrounded)
            {
                if (m_IsFloating) m_IsFloating = false;

                m_MovementDirection.y = m_StickToGround;

                if (Input_Jump)
                {
                    PlaySound(m_JumpSound);
                    m_MovementDirection.y = m_JumpForce;
                }
            }
            else
            {
                if (!m_IsFloating) m_IsFloating = true;

                if (m_CharacterController.collisionFlags == CollisionFlags.Above)
                {
                    m_MovementDirection.y = 0.0f;
                }

                m_MovementDirection.y -= m_Gravity * Time.deltaTime;
            }
        }

        public void Handle_Crouch()
        {
            if (Input_Crouch && transform.localScale.y != (m_CrouchHeight / m_CharacterController.height) * m_OriginalScale.y)
            {
                CrouchTransition(m_CrouchHeight, Time.deltaTime);
            }

            if (!Input_Crouch && transform.localScale.y != m_OriginalScale.y)
            {
                CrouchTransition(m_CharacterController.height, -Time.deltaTime);
            }

            void CrouchTransition(float TargetHeight, float value)
            {
                Vector3 Origin = transform.position + (transform.localScale.y / m_OriginalScale.y) * m_CharacterController.height * Vector3.up;
                if (Physics.Raycast(Origin, Vector3.up, m_CharacterController.height - Origin.y))
                {
                    Input_Crouch = true;
                    return;
                }

                m_CrouchTimeElapse += value;
                m_CrouchTimeElapse = Mathf.Clamp(m_CrouchTimeElapse, 0, m_CrouchDelay);

                transform.localScale = new Vector3(
                    transform.localScale.x,
                    Mathf.Lerp(m_OriginalScale.y, (m_CrouchHeight / m_CharacterController.height) * m_OriginalScale.y, m_CrouchTimeElapse / m_CrouchDelay),
                    transform.localScale.z);
            }
        }

        private void Handle_Step()
        {
            if (m_FootStepSounds.Length == 0) return;
            if (m_WalkBob.OnStep) PlaySound(m_FootStepSounds[UnityEngine.Random.Range(0, m_FootStepSounds.Length)]);
        }

        private void UpdateWalkBob()
        {
            if ((m_IsWalking && !m_IsFloating) || !m_WalkBob.BackToOriginalPosition)
            {
                float speed = m_MovementVelocity == 0 ? m_WalkSpeed : m_MovementVelocity;
                m_HeadMovement += m_WalkBob.UpdateBobValue(speed, m_WalkBob.BobRange);
            }
            else if (!m_IsWalking || !m_IdleBob.BackToOriginalPosition)
            {
                m_HeadMovement += m_IdleBob.UpdateBobValue(1, m_IdleBob.BobRange);
            }
        }

        private void PlaySound(AudioClip audioClip)
        {
            m_AudioSource.clip = audioClip;
            if (m_AudioSource.clip != null) m_AudioSource.PlayOneShot(m_AudioSource.clip);
        }
    }
}