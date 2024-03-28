﻿namespace PlusNine.Entities.Dtos.Requests
{
    public class CreateObjectiveRequest
    {
        public string ObjectiveName { get; set; } = string.Empty;
        public int CurrentAmount { get; set; }
        public int AmountToComplete { get; set; }
        public int Progress { get; set; }
        public bool Completed { get; set; } = false;
    }
}
