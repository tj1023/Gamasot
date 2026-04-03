using UnityEngine;

namespace Architecture
{
    /// <summary>
    /// 게임 진행 중 수집/선택 페이즈를 담당하는 상태
    /// </summary>
    public class PlayingState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            Debug.Log("[FSM] 수집 페이즈(PlayingState) 진입: 재료들을 덱에 추가하세요.");
            // UI 업데이트 등을 EventBus를 통해 브로드캐스팅할 수 있습니다.
        }

        public void Update(GameContext context)
        {
            // 매 프레임 입력 처리 혹은 타이머 처리 로직 구현
        }

        public void Exit(GameContext context)
        {
            Debug.Log("[FSM] 수집 페이즈 종료");
        }
    }

    /// <summary>
    /// 수집된 재료 조합을 기반으로 하드코딩 없이(Data-Driven) 시너지를 계산하고 정산하는 상태
    /// </summary>
    public class SettlementState : IState<GameContext>
    {
        public void Enter(GameContext context)
        {
            Debug.Log("[FSM] 정산 페이즈(SettlementState) 진입. 정산을 시작합니다.");
            CalculateTotalScore(context);
        }

        public void Update(GameContext context)
        {
            // 애니메이션 대기 등
        }

        public void Exit(GameContext context)
        {
            Debug.Log("[FSM] 정산 페이즈 종료");
        }

        private void CalculateTotalScore(GameContext context)
        {
            context.CurrentScore = 0;

            // 1. 기본 점수 합산
            foreach (var ingredient in context.CurrentIngredients)
            {
                if (ingredient != null)
                {
                    context.CurrentScore += ingredient.baseScore;
                }
            }

            Debug.Log($"[Settlement] 기본 점수 합계: {context.CurrentScore}");

            // 2. 시너지(특수 효과) 발동 검사
            foreach (var ingredient in context.CurrentIngredients)
            {
                if (ingredient == null) continue;

                foreach (var synergy in ingredient.synergies)
                {
                    // 조건 확인 및 효과 적용. Data-Driven 방식이므로
                    // 소스코드 변경 없이 인스펙터 상에서 조합된 효과들이 일괄 적용됩니다.
                    synergy?.EvaluateAndApply(context);
                }
            }

            Debug.Log($"[Settlement] 정산 완료. 최종 점수: {context.CurrentScore}");
        }
    }
}
