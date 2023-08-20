namespace DGames.Essentials.Editor
{
    public interface IMenuContentEditorProvider
    {
        BaseMenuItemEditor GetEditor(IMenuItem item);
    }
}