namespace ToDoApp
{
    /// <summary>
    /// <para>ItemType.Once: Will be removed when marked complete.</para>
    /// <para>ItemType.Common: Will be moved to CommonList when marked complete.</para>
    /// <para>ItemType.Toggled: Will remain in place but greyed out when marked complete.</para>
    /// <para>ItemType.Constant: Has no check box.</para>
    /// </summary>
    public enum ItemType { Once, Common, Toggled, Constant }
}
