

public interface IBlockEffect 
{
    public string EffectDescription { get; }
    public void ApplyEffect(PlayerStats stats);
}