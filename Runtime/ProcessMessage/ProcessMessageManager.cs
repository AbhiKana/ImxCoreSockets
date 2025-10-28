using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace ImxCoreSockets
{
    public class ProcessMessageManager : MonoBehaviour
    {
        public static ProcessMessageManager Instance { get; private set; }

        // Dictionaries to store 
        // Key: The unique ID from the attribute (e.g., "playerHealth")
        // Value: Information about the field or method.
        private Dictionary<MonoBehaviour, List<FieldInfo>> _processFields = new Dictionary<MonoBehaviour, List<FieldInfo>>();
        private Dictionary<MonoBehaviour, List<MethodInfo>> _processMethods = new Dictionary<MonoBehaviour, List<MethodInfo>>();

        private Dictionary<string, (MonoBehaviour, FieldInfo)> _variableLookup = new Dictionary<string, (MonoBehaviour, FieldInfo)>();
        private Dictionary<string, (MonoBehaviour, MethodInfo)> _methodLookup = new Dictionary<string, (MonoBehaviour, MethodInfo)>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            ScanForNetworkedMembers();
        }

        private void ScanForNetworkedMembers()
        {
            var allBehaviours = FindObjectsOfType<MonoBehaviour>();

            foreach (var behaviour in allBehaviours)
            {
                Type type = behaviour.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // --- Find ProcessVariables ---
                foreach (var field in fields)
                {
                    var attribute = field.GetCustomAttribute<ProcessVariableAttribute>();
                    if (attribute != null)
                    {
                        if (!_processFields.ContainsKey(behaviour))
                        {
                            _processFields[behaviour] = new List<FieldInfo>();
                        }
                        _processFields[behaviour].Add(field);

                        _variableLookup[attribute.VariableID] = (behaviour, field); //lookup
                        Debug.Log($"Found Variable: {attribute.VariableID} on {behaviour.name}");
                    }
                }

                // --- Find ProcessMethods ---
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<ProcessMethodAttribute>();
                    if (attribute != null)
                    {
                        if (!_processFields.ContainsKey(behaviour))
                        {
                            _processMethods[behaviour] = new List<MethodInfo>();
                        }
                        _processMethods[behaviour].Add(method);

                        _methodLookup[attribute.FunctionID] = (behaviour, method); //lookup
                       
                        Debug.Log($"Found Method: {attribute.FunctionID} on {behaviour.name}");
                    }
                }
            }
        }


        // Call
        public void HandleIncomingMessage(byte[] data)
        {
            var message = ProcessMessage.Deserialize(data);

            if (message.Type == ProcessMessage.MessageType.ProcessVariable)
            {
                HandleProcessVariable(message);
            }
            else if (message.Type == ProcessMessage.MessageType.ProcessMethod)
            {
                HandleProcessMethod(message);
            }
        }

        private void HandleProcessVariable(ProcessMessage message)
        {           
            if (!_variableLookup.TryGetValue(message.ID, out var targetComponent)) return;

            var (target, fieldInfo) = targetComponent;

            object deserializedValue = DeserializePayload(message.Payload, fieldInfo.FieldType);
            fieldInfo.SetValue(targetComponent, deserializedValue); //SetValue for 
        }

        private void HandleProcessMethod(ProcessMessage message)
        {
            if (!_methodLookup.TryGetValue(message.ID, out var targetComponent)) return;

            var (target, methodInfo) = targetComponent;
            var parameters = methodInfo.GetParameters();

            object[] deserializedArgs = new object[parameters.Length];

            if (parameters.Length > 0)
            {
                deserializedArgs[0] = DeserializePayload(message.Payload, parameters[0].ParameterType);
            }
            methodInfo.Invoke(targetComponent, deserializedArgs);
        }

        // A simple deserializer for basic types.
        private object DeserializePayload(byte[] payload, Type targetType)
        {
            if (targetType == typeof(int))
            {
                return BitConverter.ToInt32(payload, 0);
            }
            if (targetType == typeof(float))
            {
                return BitConverter.ToSingle(payload, 0);
            }
            if (targetType == typeof(string))
            {
                return System.Text.Encoding.UTF8.GetString(payload);
            }
            //
            return null;
        }
    }
}