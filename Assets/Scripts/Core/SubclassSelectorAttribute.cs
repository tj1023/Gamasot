using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// [SerializeReference] 필드에서 인터페이스나 추상 클래스를 상속받은
    /// 자식 클래스들을 인스펙터 드롭다운으로 선택할 수 있도록 해주는 어트리뷰트입니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}
