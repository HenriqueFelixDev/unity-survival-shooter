namespace SurvivalShooter
{
    public interface IVisitor
    {
        void Visit<T>(T visitable);
    }
}
