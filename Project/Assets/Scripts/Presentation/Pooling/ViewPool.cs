using System.Collections.Generic;
using UnityEngine;

namespace Project06.Presentation.Pooling
{
    // 프리팹 하나를 복제해 쓰는 View 풀. 꺼낼 때 켜고 반납할 때 꺼서 쌓아 둔다. 반납한 View는 파괴하지 않는다.
    // T는 GameObject나 컴포넌트(루트에 붙은 View 등)다.
    public class ViewPool<T> where T : Object
    {
        private readonly T _prefab;
        // 반납되어 꺼 둔 View
        private readonly Stack<T> _inactiveViews;

        /// <summary>
        /// 복제할 프리팹으로 빈 풀을 만든다. View는 처음 꺼낼 때 만든다.
        /// </summary>
        /// <param name="prefab">복제할 프리팹. 컴포넌트면 그 컴포넌트가 붙은 오브젝트를 복제한다.</param>
        /// <param name="defaultCapacity">반납 목록이 처음 확보하는 칸 수.</param>
        public ViewPool(T prefab, int defaultCapacity)
        {
            _prefab = prefab;
            _inactiveViews = new Stack<T>(defaultCapacity);
        }

        /// <summary>
        /// 반납된 View가 있으면 꺼내고, 없으면 프리팹을 복제해 만든다. 꺼낸 View는 켠다.
        /// </summary>
        /// <returns>켜진 View.</returns>
        public T Get()
        {
            T view = _inactiveViews.Count > 0 ? _inactiveViews.Pop() : Object.Instantiate(_prefab);
            SetActive(view, true);
            return view;
        }

        /// <summary>
        /// View를 끄고 반납 목록에 넣는다.
        /// </summary>
        /// <param name="view">이 풀에서 꺼낸 View.</param>
        public void Release(T view)
        {
            SetActive(view, false);
            _inactiveViews.Push(view);
        }

        private static void SetActive(T view, bool isActive)
        {
            if (view is GameObject gameObject)
            {
                gameObject.SetActive(isActive);
            }
            else if (view is Component component)
            {
                component.gameObject.SetActive(isActive);
            }
        }
    }
}
