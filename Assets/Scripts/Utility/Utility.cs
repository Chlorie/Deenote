public static class Utility
{
    public static void OperationCannotUndoMessage(Callback callback) =>
        MessageBox.Instance.Activate(new[] { "Notice", "请注意" },
            new[]
            {
                "This operation cannot be undone, are you sure to continue?",
                "该操作无法撤销，确定要继续吗？"
            },
            new MessageBox.ButtonInfo
            {
                callback = callback,
                texts = new[] { "Yes", "确定" }
            },
            new MessageBox.ButtonInfo { texts = new[] { "No", "返回" } });
}
