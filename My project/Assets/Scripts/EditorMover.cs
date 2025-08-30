using UnityEngine;

namespace DefaultNamespace
{

    [RequireComponent(typeof(PositionSaver))] // у объекта компонент PositionSaver
    public class EditorMover : MonoBehaviour //наследует от монобихевер: позицию объекта, задержку записи и продолжительность
    {
        private PositionSaver _save; //Поле сохранения ссылки на компонент
        private float _currentDelay; //Время задержки между записями позиции

        //todo comment: Что произойдёт, если _delay > _duration?
        //изменение положения объекта не будут сохранены
        [SerializeField, Range(0.2f, 1.0f)]
        private float _delay = 0.5f; // частота кадра, интервал между записями
        [SerializeField]
        private float _duration = 5f;//общее время записи

        public float Delay
        {
            get => _delay;
            set => _delay = Mathf.Clamp(value, 0.2f, 1.0f);

        }
        public float Duration
        {
            get => _duration;
            set => _duration = Mathf.Max(value, 0.2f);
        }

        private void Start()
        {
            if (_duration < _delay)
            {
                _duration = _delay * 5;
            }
            //todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
            //в Update эта операция будет часто повторяться , при каждом сохранении изменения положения, поэтому стираем перед началом записи движения
            _save = GetComponent<PositionSaver>(); //ищется компонент PositionSaver у объекта
            _save.Records.Clear();//чистка записей перед началом
        }

        private void Update() //в каждом кадре уменьшается общее время записи и когда оно станет меньше 0, отключится сохранение и вылезет сообщение,что все завершено
        {
            _duration -= Time.deltaTime;
            if (_duration <= 0f)
            {
                enabled = false;
                Debug.Log($"<b>{name}</b> finished", this);
                return;
            }

            //todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
            //_delay интервал времени, частота кадра, она не должна меняться. А в этой строке она получается будет уменьшаться со временем
            _currentDelay -= Time.deltaTime;//счетчик времени следующей записи
            if (_currentDelay <= 0f)
            {
                _currentDelay = _delay;//если меньше или равно 0, то равна частоте кадра
                _save.Records.Add(new PositionSaver.Data
                {
                    Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    //для расчета дельты,игровое время связанно с временем записи и определением пришло ли время для конкретной записи относительно текущего времени?
                    Time = Time.time,
                });
            }
        }
    }
}