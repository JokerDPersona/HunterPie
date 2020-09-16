﻿using HunterPie.Core.Enums;

namespace HunterPie.Core.Local
{
    public class AbnormalityInfo
    {
        public int Id { get; set; }
        public string InternalId { get; set; }
        public int Offset { get; set; }
        public AbnormalityType Type { get; set; }
        public bool IsDebuff { get; set; }
        public bool IsGearBuff { get; set; }
        public bool IsInfinite { get; set; }
        public bool IsPercentageBuff { get; set; }
        public float MaxTimer { get; set; }

        public bool HasConditions { get; set; }
        public int ConditionOffset { get; set; }

        public int Stack { get; set; }

        public string IconName { get; set; }
    }
}