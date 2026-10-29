using Project06.Entities;
using Project06.Systems;
using Project06.UserInterface.Ammo.Presenters;
using Project06.UserInterface.DayNight.Presenters;
using Project06.UserInterface.Health.Presenters;
using Project06.UserInterface.Mentality.Presenters;

namespace Project06.Core
{
    // UI Presenter를 찾고, 매 프레임 World에서 꺼낸 값을 넘긴다.
    public partial class Runner
    {
        // 씬에 없으면 null이며, 그 Presenter에는 값을 넘기지 않는다.
        private HealthPresenter _healthPresenter;
        private MentalityPresenter _mentalityPresenter;
        private AmmoPresenter _ammoPresenter;
        private DayNightPresenter _dayNightPresenter;

        // 씬에 배치한 UI Presenter를 찾는다. 판 시작 시 한 번 호출한다.
        private void InitializeUserInterface()
        {
            _healthPresenter = FindFirstObjectByType<HealthPresenter>();
            _mentalityPresenter = FindFirstObjectByType<MentalityPresenter>();
            _ammoPresenter = FindFirstObjectByType<AmmoPresenter>();
            _dayNightPresenter = FindFirstObjectByType<DayNightPresenter>();
        }

        // 표시할 플레이어와 낮밤의 값을 UI Presenter에 넘긴다. 인덱스가 음수면 그 엔티티에 해당하는 Presenter는 건너뛴다.
        private void PresentUserInterface(int playerIndex, int dayNightIndex)
        {
            if (playerIndex >= 0)
            {
                ref PlayerEntity player = ref _world.players.entities[playerIndex];
                if (_healthPresenter != null)
                {
                    _healthPresenter.Present(player.health, player.player.maxHealth);
                }

                if (_mentalityPresenter != null)
                {
                    _mentalityPresenter.Present(player.mentality, player.player.maxMentality);
                }

                if (_ammoPresenter != null)
                {
                    _ammoPresenter.Present(player.magazineAmmo, GetReserveAmmo(in player));
                }
            }

            if (dayNightIndex >= 0 && _dayNightPresenter != null)
            {
                ref DayNightEntity dayNight = ref _world.dayNights.entities[dayNightIndex];
                _dayNightPresenter.Present(dayNight.dayCount, dayNight.isNight, dayNight.remainingTicks, DayNightSystem.IsFinalNight(in dayNight));
            }
        }

        // 지금 구경의 예비 탄약 수를 구한다. 구경이 없거나 범위를 벗어나면 0이다.
        private static int GetReserveAmmo(in PlayerEntity player)
        {
            return player.reserveAmmo != null && player.caliberIndex >= 0 && player.caliberIndex < player.reserveAmmo.Length
                ? player.reserveAmmo[player.caliberIndex]
                : 0;
        }
    }
}
