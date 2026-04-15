namespace Interfaces
{
    /// <summary>
    /// 비동기 실행 가능한 커맨드 (UI 애니메이션 및 딜레이 포함)
    /// </summary>
    public interface ICommand
    {
        System.Collections.IEnumerator ExecuteAsync();
    }
}
