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
                    ""name"": ""Forwards"",
                    ""type"": ""Button"",
                    ""id"": ""a50ad648-3357-4a81-833d-68f3492a58ab"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Backwards"",
                    ""type"": ""Button"",
                    ""id"": ""99a6acb1-30ee-4736-88d9-7085852c91f7"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""0b5cc24f-20ba-40b7-80d3-55a081ea2b1c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Forwards"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""083c7b53-32db-44be-8c9a-cb0af66a567d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Backwards"",
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
        m_TestPlayer_Forwards = m_TestPlayer.FindAction("Forwards", throwIfNotFound: true);
        m_TestPlayer_Backwards = m_TestPlayer.FindAction("Backwards", throwIfNotFound: true);
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
    private readonly InputAction m_TestPlayer_Forwards;
    private readonly InputAction m_TestPlayer_Backwards;
    public struct TestPlayerActions
    {
        private @NewControls m_Wrapper;
        public TestPlayerActions(@NewControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Forwards => m_Wrapper.m_TestPlayer_Forwards;
        public InputAction @Backwards => m_Wrapper.m_TestPlayer_Backwards;
        public InputActionMap Get() { return m_Wrapper.m_TestPlayer; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(TestPlayerActions set) { return set.Get(); }
        public void SetCallbacks(ITestPlayerActions instance)
        {
            if (m_Wrapper.m_TestPlayerActionsCallbackInterface != null)
            {
                @Forwards.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnForwards;
                @Forwards.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnForwards;
                @Forwards.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnForwards;
                @Backwards.started -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnBackwards;
                @Backwards.performed -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnBackwards;
                @Backwards.canceled -= m_Wrapper.m_TestPlayerActionsCallbackInterface.OnBackwards;
            }
            m_Wrapper.m_TestPlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Forwards.started += instance.OnForwards;
                @Forwards.performed += instance.OnForwards;
                @Forwards.canceled += instance.OnForwards;
                @Backwards.started += instance.OnBackwards;
                @Backwards.performed += instance.OnBackwards;
                @Backwards.canceled += instance.OnBackwards;
            }
        }
    }
    public TestPlayerActions @TestPlayer => new TestPlayerActions(this);
    public interface ITestPlayerActions
    {
        void OnForwards(InputAction.CallbackContext context);
        void OnBackwards(InputAction.CallbackContext context);
    }
}
