#nullable enable

using Deenote.Entities.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Runtime.Plugins.ChartConversion.Rpe
{
    public class RpeConverter
    {
        private const int UsingRpeVersion = 170;

        private const int TapType = 1;
        private const int HoldType = 2;
        private const int FlickType = 3;
        private const int DragType = 4;

        private const float MaxDeemoStagePosition = 2f;
        private const float MaxRpeStagePosition = 450f;
        private const float MainLineFloorPosition = -300f;
        private const float MainLineUpperLength = 7f;
        private const float HoldDragLineFloorPosition = 1000f;

        public float Speed { get; set; } = 10f;

        public float SpeedCoefficient { get; set; } = 1f;

        public float SpeedExponent { get; set; } = 1f;

        public float WidthCoefficient { get; set; } = 1f;

        public float WidthExponent { get; set; } = 1f;

        public float WidthMultiplier { get; set; } = 1f;

        public bool NeedFlickClick { get; set; } = true;

        public int HoldAlpha { get; set; } = 165;

        public bool EarlyDisplaySlowNotes { get; set; } = false;

        public float HoldDragInterval { get; set; } = 0.08f;

        /// <returns> rpe chart with uninitialized <see cref="RpeChart.META"/> </returns>
        public RpeChart Convert(ChartModel deemoChart)
        {
            var mainNotes = new List<Note>();
            var holdDragNotes = new List<Note>();

            float maxAppearTime = float.NegativeInfinity;
            // Assume the enumerated notes are sorted by time
            foreach (var de2Note in deemoChart.EnumerateNoteModels()) {
                if (Mathf.Abs(de2Note.Position) > MaxDeemoStagePosition + 0.001f) continue;

                float noteSpeed = CalculateNoteSpeed(de2Note.Speed);
                float noteSize = CalculateNoteSize(de2Note.Size);
                float appearTimeCalculated = CalculateAppearTime(de2Note.Time, noteSpeed);
                float appearTimeInChart = appearTimeCalculated;

                if (!EarlyDisplaySlowNotes) {
                    if (maxAppearTime > appearTimeInChart) appearTimeInChart = maxAppearTime;
                    if (appearTimeCalculated > maxAppearTime) maxAppearTime = appearTimeCalculated;
                }

                float positionX = de2Note.Position * MaxRpeStagePosition / MaxDeemoStagePosition;
                float visibleTime = de2Note.EndTime - appearTimeInChart;

                int noteType = de2Note.Kind switch {
                    NoteModel.NoteKind.Click => TapType,
                    NoteModel.NoteKind.Slide => DragType,
                    NoteModel.NoteKind.Swipe => FlickType,
                    _ => TapType
                };

                mainNotes.Add(new Note(noteType, positionX, noteSize, Beat.GetMilliBeat(de2Note.Time)) {
                    speed = noteSpeed, visibleTime = visibleTime,
                });

                if (noteType == FlickType && NeedFlickClick) {
                    mainNotes.Add(new Note(TapType, positionX, noteSize, Beat.GetMilliBeat(de2Note.Time)) {
                        above = 0, speed = noteSpeed, visibleTime = visibleTime,
                    });
                }

                // All hold body are fake and is replaced by a series of drag notes
                if (de2Note.GetActualDuration() > 0f) {
                    mainNotes.Add(new Note(HoldType, positionX, noteSize,
                        Beat.GetMilliBeat(de2Note.Time), Beat.GetMilliBeat(de2Note.EndTime)) {
                        isFake = 1,
                        speed = noteSpeed,
                        judgeArea = noteSize,
                        visibleTime = visibleTime,
                        alpha = HoldAlpha
                    });

                    var time = de2Note.Time;
                    for (; time <= de2Note.EndTime; time += HoldDragInterval) {
                        holdDragNotes.Add(new Note(DragType, positionX, noteSize, Beat.GetMilliBeat(time)) {
                            speed = noteSpeed, visibleTime = visibleTime, alpha = 0
                        });
                    }
                }
            }

            var mainJudgeLine = CreateJudgeLine("BaseLine", MainLineFloorPosition, mainNotes);
            var holdDragJudgeLine = CreateJudgeLine("HoldDragLine", HoldDragLineFloorPosition, holdDragNotes);

            return new RpeChart {
                META = new META { RPEVersion = UsingRpeVersion },
                BPMList = new List<BPMListItem> { new() { startTime = new Beat(0, 0, 1), bpm = Beat.DefaultBpm } },
                judgeLineList = new() { mainJudgeLine, holdDragJudgeLine },
                judgeLineGroup = new() { "Default" }
            };
        }

        private float CalculateNoteSpeed(float originalSpeed)
            => SpeedCoefficient * Mathf.Pow(originalSpeed, SpeedExponent) + 1f - SpeedCoefficient;

        private float CalculateNoteSize(float originalSize)
            => (WidthCoefficient * Mathf.Pow(originalSize, WidthExponent) + 1f - WidthCoefficient) * WidthMultiplier;

        private float CalculateAppearTime(float noteTime, float noteSpeed)
            => noteTime - MainLineUpperLength / noteSpeed / Speed;

        private JudgeLine CreateJudgeLine(string name, float floorPosition, List<Note> notes)
        {
            return new JudgeLine {
                numOfNotes = notes.Count,
                eventLayers = new List<EventLayer> { EventLayer.Default(floorPosition, Speed) },
                extended = Extended.Default(),
                notes = notes,
                Group = 0,
                Name = name,
                zOrder = 0
            };
        }
    }
}