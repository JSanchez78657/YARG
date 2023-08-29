using System;
using TMPro;
using UnityEngine;
using YARG.Core.Chart;
using YARG.Menu;

namespace YARG.Gameplay.HUD
{
    public class ScoreBox : GameplayBehaviour
    {
        private const string SCORE_PREFIX = "<mspace=0.538em>";

        private const string TIME_FORMAT       = "m\\:ss";
        private const string TIME_FORMAT_HOURS = "h\\:mm\\:ss";

        [SerializeField]
        private TextMeshProUGUI _scoreText;
        [SerializeField]
        private StarDisplay _starDisplay;

        [Space]
        [SerializeField]
        private ProgressBarFadedEdge _songProgressBar;
        [SerializeField]
        private TextMeshProUGUI _songTimer;

        private int _bandScore;

        private bool _songHasHours;
        private string _songLengthTime;

        private string TimeFormat => _songHasHours ? TIME_FORMAT_HOURS : TIME_FORMAT;

        private void Start()
        {
            _scoreText.text = SCORE_PREFIX + "0";
            _songTimer.text = string.Empty;
        }

        protected override void OnSongStarted()
        {
            var timeSpan = TimeSpan.FromSeconds(GameManager.SongLength);
            _songHasHours = timeSpan.TotalHours >= 1.0;
            _songLengthTime = timeSpan.ToString(TimeFormat);
        }

        private void Update()
        {
            if (GameManager.Paused)
            {
                return;
            }

            // Update score

            if (GameManager.BandScore != _bandScore)
            {
                _bandScore = GameManager.BandScore;
                _scoreText.text = SCORE_PREFIX + _bandScore.ToString("N0");

                UpdateStars();
            }

            // Update song progress

            // Skip if the song length has not been established yet
            if (_songLengthTime == null) return;

            double time = Math.Max(0f, GameManager.SongTime);

            _songProgressBar.SetProgress((float) (time / GameManager.SongLength));

            string currentTime = TimeSpan.FromSeconds(time).ToString(TimeFormat);
            _songTimer.text = $"{currentTime} / {_songLengthTime}";
        }

        private void UpdateStars()
        {
            double totalStarCount = 0;

            foreach (var player in GameManager.Players)
            {
                if (player.StarScoreThresholds[0] == 0)
                {
                    continue;
                }

                int fullStars = 5;
                while (fullStars >= 0 && player.Score < player.StarScoreThresholds[fullStars])
                {
                    fullStars--;
                }

                fullStars++;

                totalStarCount += fullStars;

                double progressToNextStar = 0;
                if (fullStars == 0)
                {
                    progressToNextStar = player.Score / (double)player.StarScoreThresholds[fullStars];
                }
                else if (fullStars < 6)
                {
                    int previousStarThreshold = player.StarScoreThresholds[fullStars - 1];
                    progressToNextStar = (player.Score - previousStarThreshold) /
                        (double)(player.StarScoreThresholds[fullStars] - previousStarThreshold);
                }

                totalStarCount += progressToNextStar;
            }

            _starDisplay.SetStars(totalStarCount);
        }
    }
}