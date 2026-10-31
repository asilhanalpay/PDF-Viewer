using PdfViewer.Core;
using Xunit;
namespace PdfViewer.Tests;
public class DocumentPolicyTests
{
 [Theory]
 [InlineData(612,792,4)] [InlineData(100000,1,3)] [InlineData(1,100000,3)]
 public void RasterIsBoundedAndNonzero(double width,double height,double scale)
 {
  var size=DocumentPolicy.RasterSize(width,height,scale);
  Assert.InRange(size.Width,1u,2560u); Assert.InRange(size.Height,1u,2560u);
 }
 [Fact] public void InvalidDimensionsRejected() => Assert.Throws<ArgumentOutOfRangeException>(()=>DocumentPolicy.RasterSize(double.NaN,100,1));
 [Theory] [InlineData(-1,10,1)] [InlineData(100,10,10)] [InlineData(1,0,0)]
 public void NavigationStaysWithinDocument(int input,int total,int expected) => Assert.Equal(expected,DocumentPolicy.ClampPage(input,total));
 [Fact] public void NonfiniteZoomDoesNotReachRenderer() => Assert.Equal(1,DocumentPolicy.ClampZoom(double.NaN));
}
