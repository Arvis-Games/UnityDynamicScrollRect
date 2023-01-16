using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace pooling
{
    public class Pooling<T> : List<T> where T : MonoBehaviour, IPooling
    {
        public bool createMoreIfNeeded = true;

        private Transform mParent;
        private Vector3 mStartPos;
        private GameObject referenceObject;

        public delegate void ObjectCreationCallback(T obj);
        public event ObjectCreationCallback OnObjectCreationCallBack;

        private DiContainer _container;
        
        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }
        
        public Pooling<T> Initialize(GameObject refObject, Transform parent)
        {
            return Initialize(0, refObject, parent);
        }

        public Pooling<T> Initialize(int amount, GameObject refObject, Transform parent, bool startState = false)
        {
			return Initialize(amount, refObject, parent, Vector3.zero, startState);
        }

        public Pooling<T> Initialize(int amount, GameObject refObject, Transform parent, Vector3 worldPos, bool startState = false)
        {
            mParent = parent;
            mStartPos = worldPos;
            referenceObject = refObject;

            Clear();

            for (var i = 0; i < amount; i++)
            {
                var obj = CreateObject();

                if(startState) obj.OnCollect();
                else obj.OnRelease();

                Add(obj);
            }

            return this;
        }
        
        public T Collect(Transform parent = null, Vector3? position = null, bool localPosition = true)
        {
            var obj = Find(x => x.isUsing == false);
            if (obj == null && createMoreIfNeeded)
            {
                obj = CreateObject(position);
                Add(obj);
            }

            if (obj == null) return obj;

            obj.transform.SetParent(parent ?? mParent);
            if (localPosition)
                obj.transform.localPosition = position ?? mStartPos;
            else
                obj.transform.position = position ?? mStartPos;
            obj.OnCollect();

            return obj;
        }

        public void Release(T obj)
        {
			if(obj != null)
                obj.OnRelease();
        }

        public List<T> GetAllWithState(bool active)
        {
            return FindAll(x => x.isUsing == active);
        }

        private T CreateObject(Vector3? position = null)
        {
            var go = _container.InstantiatePrefab(referenceObject, position ?? mStartPos, Quaternion.identity, mParent);
            var obj = go.GetComponent<T>() ?? go.AddComponent<T>();
            var objTransform = obj.transform;
            objTransform.localPosition = position ?? mStartPos;
            objTransform.localScale = Vector3.one;
            obj.name = obj.objectName + Count;

			if(OnObjectCreationCallBack != null)
                OnObjectCreationCallBack.Invoke(obj);

            return obj;
        }
    }
}