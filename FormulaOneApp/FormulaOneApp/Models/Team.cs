namespace FormulaOneApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string TeamPrinciple { get; set; }
        public ICollection<Pilot> Pilots { get; set; } = new List<Pilot>();
    }
}
