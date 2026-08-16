namespace Domain.Entities
{
    public class Modifiers
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } 
        public string? Description { get; private set; } 
        public MenuItem MenuItem { get; private set; } 
        public Guid MenuItemId { get; private set; }
        public bool IsAvailable { get; private set; }


        private Modifiers ()
        {
        }
        private Modifiers (Guid itemId , string title , string description)
        {
            Id = Guid.NewGuid ();
            Title = title; 
            Description = description; 
            MenuItemId = itemId;
            IsAvailable = true;
        }

        public static Modifiers Create(Guid itemId , string description , string title)
        {

            return new Modifiers (itemId , title , description);
        }
        public void Update(string title , string description , bool isAvailable)
        {
            Title = title; 
            Description = description; 
            IsAvailable = isAvailable; 
        }
    }
}
