using System;
using System.Globalization;
using System.Linq;
using Cinemachine;
using UnityEngine;

namespace CustomBeatmaps.InvestigationUtils
{
    public class FirstPersonFreecam : MonoBehaviour
    {
        private CinemachineVirtualCamera _vcam;

        private Rect _debugWindow = new Rect(10, 10, 300, 400);

        private Vector3 _lastMousePos;
        
        private float _moveSpeed = 1;
        private float _lookSensitivity = 0.5f;

        private float _lookX, _lookY;

        public static void CreateInScene()
        {
            GameObject newObj = new GameObject("FREECAM");
            newObj.AddComponent<CinemachineVirtualCamera>();
            newObj.AddComponent<FirstPersonFreecam>();
        }
        private void Awake()
        {
            Debug.Log("FIRST PERSON CAMERA CREATED");
            _vcam = GetComponent<CinemachineVirtualCamera>();
        }

        private void Update()
        {
            if (_vcam != null)
            {
                _vcam.Priority = 999999;
            }

            if (Input.GetMouseButton(1))
            {
                Vector3 moveVector = MoveAxis(KeyCode.W, KeyCode.S) * transform.forward
                                     + MoveAxis(KeyCode.D, KeyCode.A) * transform.right
                                     + MoveAxis(KeyCode.E, KeyCode.Q) * transform.up;
                transform.position += moveVector * (Time.deltaTime * _moveSpeed);

                Vector3 delta = Input.mousePosition - _lastMousePos;
                _lastMousePos = Input.mousePosition;
                Debug.Log(delta);

                _lookX += delta.x * _lookSensitivity;
                _lookY -= delta.y * _lookSensitivity;
                transform.rotation = Quaternion.Euler(_lookY, _lookX, 0);
            }
            else
            {
                _lastMousePos = Input.mousePosition;
            }
        }

        private void OnGUI()
        {
            _debugWindow = GUI.ModalWindow(0, _debugWindow, id =>
            {
                GUILayout.BeginArea(_debugWindow);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Position");
                transform.position = Vector3Field(transform.position);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Move Speed");
                _moveSpeed = FloatField(_moveSpeed);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Look Sensitivity");
                _lookSensitivity = FloatField(_lookSensitivity);
                GUILayout.EndHorizontal();
                GUILayout.Label(transform.eulerAngles.ToString());
                GUILayout.EndArea();
            }, "Free Cam");
        }

        private static float MoveAxis(KeyCode positive, KeyCode negative)
        {
            return (Input.GetKey(positive) ? 1 : 0) +
                   (Input.GetKey(negative) ? -1 : 0);
        }
        private static Vector3 Vector3Field(Vector3 value)
        {
            Vector3 result = value;
            result.x = FloatField(result.x);
            result.y = FloatField(result.y);
            result.z = FloatField(result.z);
            return result;
        }
        private static float FloatField(float value)
        {
            string converted = value.ToString(CultureInfo.InvariantCulture);
            if (converted.IndexOf('.') == -1)
            {
                converted += '.';
            }
            string val = GUILayout.TextField(converted);
            if (float.TryParse(val, out float result))
            {
                return result;
            }
            return value;
        }
    }
}
