using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class PositionSaver : MonoBehaviour //класс хранит в себе позиции объекта связан с реплей муви
    {
        [Serializable]
        public struct Data
        {
            public Vector3 Position;
            public float Time;
        }
        [ReadOnly]
        [Tooltip("Для заполнения поля нужно воспользоваться контекстным меню в инспекторе и командой Create File")]
        private TextAsset _json; //ассет по позициям объекта

        [SerializeField, HideInInspector]
        private List<Data> _records = new List<Data>(); //сохраненные данные по позициям объекта в файле

        public List<Data> Records => _records; //автосвойство, только для чтения

        private void Awake()
        {
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            //в условии идет проверка наличия ассета по позициям объекта , если его нет отключаем объект, сообщаем о ошибке, закрываем метод. если не выйти то если ассет будет отсутствовать, программа будет дальше делать десериализацию и выдаст ошибку
            if (_json == null)
            {
                gameObject.SetActive(false);
                Debug.LogError("Please, create TextAsset and add in field _json");
                return;
            }

            JsonUtility.FromJsonOverwrite(_json.text, this);//передача данных из ассета в текущий объект
                                                            //todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
                                                            //если данных нет, то создается новый список.Если нет ассета, то мы закрыли предыдущий метод передачи данных о положении объекта и начали записывать новые данные. Без этого новые данные записываться не будут.
            if (_records == null)
                _records = new List<Data>(10);
        }

        private void OnDrawGizmos()
        {
            //todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
            //если нет данных об объекте, то закрываем метод. Если данные есть, то по этим позициям рисуем сферы и получаем отображение траектории цветом
            if (_records == null || _records.Count == 0) return;

            var data = _records;
            var prev = data[0].Position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(prev, 0.3f);
            //todo comment: Почему итерация начинается не с нулевого элемента?
            //нулевое значение , это базовое значение до начала изменения положения объекта т.е для нулевого значения мы уже выше создаем зеленый кружок var prev = data[0].Position
            for (int i = 1; i < data.Count; i++)
            {
                var curr = data[i].Position;
                Gizmos.DrawWireSphere(curr, 0.3f);
                Gizmos.DrawLine(prev, curr);
                prev = curr;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create File")]//создание файла в папке объекта
        private void CreateFile()
        {
            //todo comment: Что происходит в этой строке?
            //создание самого файла
            var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
            //todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав)
            //закрытие файла и обновление данных ассета (при закрытии эти данные автоматически обновляются, на уроке говорили)
            stream.Dispose();
            UnityEditor.AssetDatabase.Refresh();
            //В Unity можно искать объекты по их типу, для этого используется префикс "t:"
            //После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
            var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
            foreach (var guid in guids)
            {
                //Этой командой можно получить путь к ассету через его гуид
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                //Этой командой можно загрузить сам ассет
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                //проверяем имя файла и он становится ассетом джейсон, сохраняем изменения
                if (asset != null && asset.name == "Path")
                {
                    _json = asset;
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    //todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
                    //у нас уже есть ассет, дальше искать уже ничего не нужно
                    return;
                }
            }
        }

        private void OnDestroy()
        {
            if (_json != null)
            {
                // Создаем объект для сериализации
                var wrapper = new RecordsWrapper { Records = _records };
                string jsonText = JsonUtility.ToJson(wrapper, true);

                // Записываем JSON в ассет
                string path = UnityEditor.AssetDatabase.GetAssetPath(_json);
                try
                {
                    File.WriteAllText(path, jsonText);
                    UnityEditor.EditorUtility.SetDirty(_json);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to save JSON to {_json.name}: {e.Message}");
                }
            }
        }

        [Serializable]
        private class RecordsWrapper
        {
            public List<Data> Records;
        }
#endif
    }
}
