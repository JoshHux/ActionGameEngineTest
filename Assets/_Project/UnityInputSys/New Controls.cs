// GENERATED AUTOMATICALLY FROM 'Assets/_Project/UnityInputSys/New Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @NewControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @NewControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""New Controls"",
    ""maps"": [
        {
            ""name"": ""TestPlayer"",
            ""id"": ""208adfb1-aa48-4321-b860-8a018e2004e0"",
            ""actions"": [
                {
                    ""name"": ""Direction"",
                    ""type"": ""Value"",
                    ""id"": ""c7ed705a-d032-47d0-b6fd-b7621c0b1d9e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Punch"",
                    ""type"": ""Button"",
                    ""id"": ""83702c10-ae4e-4bd2-b0ff-ccc4fd1bc9b0"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Kick"",
                    ""type"": ""Button"",
                    ""id"": ""a3e8de3c-0ea3-4eae-ab98-20e4f8895e2c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Slash"",
                    ""type"": ""Button"",
                    ""id"": ""4e89d383-c9e1-4554-87c2-f860c3a6714c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dust"",
                    ""type"": ""Button"",
                    ""id"": ""12bc5104-5a0d-479e-bf45-a844b32cb604"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""4c333ec4-cd69-45e2-a6b7-0e0ce2046606"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""f407d213-1982-4310-a209-5b2014daac09"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""5d995614-7367-4ca2-b74b-8c8c448e6f12"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7607a2f3-d159-4c43-879d-c69aefa4f940"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c263b215-1ef9-4dc3-874e-4d50d23c47fc"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""35c8618e-c84d-4280-b70a-66d86c045fb6"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Direction"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""e9648e88-34ec-4af0-a38b-7ad73c3c6b2e"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Punch"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4a48e29e-1e21-42fa-9ce2-570c2c5c08e3"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Kick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""56d4937e-f196-4313-900e-9cb79564fa0b"",
                    ""path"": ""<Keyboard>/l"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Slash"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b022a629-f59d-4571-9e46-545bb2bc2558"",
                    ""path"": ""<Keyboard>/semicolon"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dust"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d686fdfe-febb-48a3-b2a6-a84ac2346add"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // TestPlayer
        m_TestPlayer = asset.FindActionMap("TestPlayer", throwIfNotFound: true);
        m_TestPlayer_Direction = m_TestPlayer.FindAction("Direction", throwIfNotFound: true);
        m_TestPlayer_Punch = m_TestPlayer.FindAction("Punch", throwIfNotFound: true);
        m_TestPlayer_Kick = m_TestPlayer.FindAction("Kick", throwIfNotFound: true);
        m_TestPlayer_Slash = m_TestPlayer.FindAction("Slash", throwIfNotFound: true);
        m_TestPlayer_Dust = m_TestPlayer.FindAction("Dust", throwIfNotFound: true);
        m_TestPlayer_Jump = m_TestPlayer.FindAction("Jump", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // TestPlayer
    private readonly InputActionMap m_TestPlayer;
    private ITestPlayerActions m_TestPlayerActionsCallbackInterface;
    private readonly InputAction m_TestPlayer_Direction;
    private readonly InputAction m_TestPlayer_Punch;
    private readonly InputAction m_TestPlayer_Kick;
    private readonly InputAction m_TestPlayer_Slash;
    private readonly InputAction m_TestPlayer_Dust;
    private readonly InputAction m_TestPlayer_Jump;
    public struct TestPlayerActions
    {
        private @NewControls m_Wrapper;
        public TestPlayerActions(@NewControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Direction => m_Wrapper.m_TestPlayer_Direction;
        public InputAction @Punch => m_Wrapper.m_TestPlayer_Punch;
        public InputAction @Kick => m_Wrapper.m_TestPlayer_Kick;
        public InputAction @Slash => m_Wrapper.m_TestPlayer_Slash;
        public InputAction @Dust => m_Wrapper.m_TestPlayer_Dust;
        public InputAction @Jump => m_Wrapper.m_TestPlayer_Jump;
        public InputActionMap Get() { return m_Wrapper.m_TestPlayer; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TestPlayerActions set) { return set.Get(); }
        public void SetCallbacks(ITestPlayerActions instance)
        {
            if (m_Wrapper.m_TestPlayerActionsCallbackInterface != null)
            {
                @Direction.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDirection;
                @Direction.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDirection;
                @Direction.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDirection;
                @Punch.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnPunch;
                @Punch.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnPunch;
                @Punch.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnPunch;
                @Kick.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnKick;
                @Kick.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnKick;
                @Kick.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnKick;
                @Slash.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnSlash;
                @Slash.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnSlash;
                @Slash.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnSlash;
                @Dust.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDust;
                @Dust.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDust;
                @Dust.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnDust;
                @Jump.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_TestPlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Direction.started += instance.OnDirection;
                @Direction.performed += instance.OnDirection;
                @Direction.canceled += instance.OnDirection;
                @Punch.started += instance.OnPunch;
                @Punch.performed += instance.OnPunch;
                @Punch.canceled += instance.OnPunch;
                @Kick.started += instance.OnKick;
                @Kick.performed += instance.OnKick;
                @Kick.canceled += instance.OnKick;
                @Slash.started += instance.OnSlash;
                @Slash.performed += instance.OnSlash;
                @Slash.canceled += instance.OnSlash;
                @Dust.started += instance.OnDust;
                @Dust.performed += instance.OnDust;
                @Dust.canceled += instance.OnDust;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public TestPlayerActions @TestPlayer => new TestPlayerActions(this);
    public interface ITestPlayerActions
    {
        void OnDirection(InputAction.CallbackContext context);
        void OnPunch(InputAction.CallbackContext context);
        void OnKick(InputAction.CallbackContext context);
        void OnSlash(InputAction.CallbackContext context);
        void OnDust(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
