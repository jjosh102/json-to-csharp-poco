// using JsonToCsharpPoco.Converter;
// using Xunit;

// public class PascalCaseTests
// {

//   private readonly CSharpPocoBuilder _cSharpPocoBuilder;

//   public PascalCaseTests()
//   {
//     _cSharpPocoBuilder = new();
//   }

//   [Theory]
//   [InlineData("hello_world", "HelloWorld")]
//   [InlineData("test_case_example", "TestCaseExample")]
//   [InlineData("pascal_case", "PascalCase")]
//   [InlineData("singleword", "Singleword")]
//   [InlineData("alreadyPascalCase", "AlreadyPascalCase")]
//   public void ToPascalCase_BasicTests(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("", "")]
//   [InlineData(null, null)]
//   [InlineData("a", "A")]
//   [InlineData("A", "A")]
//   [InlineData("1", "1")]
//   [InlineData("1_test_case", "1TestCase")]
//   public void ToPascalCase_EdgeCases(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("abc_123_test", "Abc123Test")]
//   [InlineData("test_123_case", "Test123Case")]
//   [InlineData("number_1_example", "Number1Example")]
//   public void ToPascalCase_WithNumbers(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("test@case#example", "TestCaseExample")]
//   [InlineData("hello!world?", "HelloWorld")]
//   [InlineData("test_case$%^&*()", "TestCase")]
//   public void ToPascalCase_WithSpecialCharacters(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("test__case", "TestCase")]
//   [InlineData("_test_case", "TestCase")]
//   [InlineData("test_case_", "TestCase")]
//   [InlineData("__test_case__", "TestCase")]
//   public void ToPascalCase_WithUnderscores(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("this_is_a_very_long_string_that_should_be_pascal_case", "ThisIsAVeryLongStringThatShouldBePascalCase")]
//   [InlineData("another_example_with_long_text_for_testing_purpose", "AnotherExampleWithLongTextForTestingPurpose")]
//   public void ToPascalCase_LongStrings(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }

//   [Theory]
//   [InlineData("123_test_case", "123TestCase")]
//   [InlineData("456_example_case", "456ExampleCase")]
//   public void ToPascalCase_WithNumberAtStart(string input, string expected)
//   {
//     var result = _cSharpPocoBuilder.ToPascalCase(input);
//     Assert.Equal(expected, result);
//   }


// }

