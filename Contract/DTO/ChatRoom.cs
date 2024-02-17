namespace de.hsfl.vs.hul.chatApp.contract.DTO;

public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public override bool Equals(object obj)
    {
        return obj is ChatRoom chatroom && Id == chatroom.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}