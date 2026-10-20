public class DocumentStatusTransitionTests
{
    [Fact]
    public void StartProcessing_WhenPending_MovesToProcessing()
    {
        // Arrange
        var document = DocumentFactory.CreateInStatus(DocumentStatus.Pending);

        // Act
        var result = document.StartProcessing(DocumentFactory.Now);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentStatus.Processing, document.Status);
    }


}