using System;
using UnityEditor;
using UnityEngine;

namespace DefaultNamespace
{
    public class ReadOnlyAttribute : PropertyAttribute
    { }


    public class Editor_ReadOnlyAttribute : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    [RequireComponent(typeof(PositionSaver))] //отражает движение объекта по записи
    public class ReplayMover : MonoBehaviour
    {
        private PositionSaver _save;

        private int _index;
        private PositionSaver.Data _prev;
        private float _duration;

        private void Start() //проверяем наличие компонента и записи, если нет отключаем галочку компонента
        {
            //todo comment: зачем нужны эти проверки?
            //условие :нет компонента или нет сохранений, запускать нечего значит некорректное значение и будет ошибка, чтобы ошибки не было, отключаем галочку
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
            {
                Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                //чтобы избежать ошибок
                enabled = false;
            }
        }

        private void Update()
        {
            var curr = _save.Records[_index]; // берем текущую запись
                                              //todo comment: Что проверяет это условие (с какой целью)? 
                                              // проверка на время, проверяем пришло ли время для этой записи : если время больше времени текущей записи, то эта запись становится предыдущей и увеличивается индекс
            if (Time.time > curr.Time)
            {
                _prev = curr;
                _index++;
                //todo comment: Для чего нужна эта проверка?
                // если индекс будет больше самого большого сохраненного значения, то мы дошли до конца списка и отключаем компонент
                if (_index >= _save.Records.Count)
                {
                    enabled = false;
                    Debug.Log($"<b>{name}</b> finished", this);
                }
            }
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            // дельта времени между предыдущей записью и текущей, далее применяется для обновления позиции объекта между этими записями. То есть я понимаю, что в данном случае есть точка "а" и "б" как предыдущая и текущая запись, а обновление отображения/ позиции объекта происходит в "с" , которая межу точками а и б по времени по времени
            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
            //todo comment: Зачем нужна эта проверка?
            //наверное, чтобы избежать ошибок связанных с 0
            if (float.IsNaN(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
            //Lerp - линейная интерполяция от а до б по дельте смещения (по определению из урока). Получается тут позиция объекта меняется по вектору интерполяция, точнее по дельте вектора.
        }
    }
}
