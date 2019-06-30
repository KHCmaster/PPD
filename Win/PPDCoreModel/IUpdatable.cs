namespace PPDCoreModel
{
    public interface IUpdatable : IPriority
    {
        void Update(float movieTime);
    }
}
