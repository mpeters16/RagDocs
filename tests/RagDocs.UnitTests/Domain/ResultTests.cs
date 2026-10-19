using RagDocs.Domain.Common;

public class ResultTests
{
      private static readonly Error TestError =
        Error.Validation("Test.Error", "Something went wrong.");
    
    [Fact]
    public void Success_ReturnsSuccessWithNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

     [Fact]
    public void Failure_ReturnsFailureWithError()
    {
        // Act
        var result = Result.Failure(TestError);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(TestError, result.Error);
    }


    [Fact]
    public void Failure_WithErrorNone_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void ImplicitConversion_FromError_ReturnsFailure()
    {
        //Act
         Result result= TestError;

        //Assert
         Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

     [Fact]
    public void GenericSuccess_ReturnsValue()
    {
        // Act
        var result = Result<int>.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void GenericFailure_ReturnsError()
    {
        //Act
        var result = Result<int>.Failure(TestError);

        //Assert
        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }


    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure(TestError);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void GenericFailure_WithErrorNone_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result<int>.Failure(Error.None));
    }

    [Fact]
    public void ImplicitConversion_FromValue_ReturnsSuccessWithValue()
    {
        // Act
        Result<string> result = "hello";

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ReturnsGenericFailure()
    {
        // Act
        Result<string> result = TestError;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }
}