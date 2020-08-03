namespace BDO.Core.Commands
{
    public class AddItem : ZES.Infrastructure.Domain.Command, ZES.Interfaces.Domain.ICreateCommand
    {
        public AddItem() 
        {
        } 
    
        public AddItem(string name, int grade) 
            : base($"{name}_{grade}")
        {
            Grade = grade;
            Name = name;
        }
    
        public int Grade { get; } 
        public string Name { get; }
    }
}