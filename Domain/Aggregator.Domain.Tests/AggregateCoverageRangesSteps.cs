using System.Linq;
using Aggregator.Contracts;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Aggregator.Domain.Tests
{
    public class AggregateCoverageRangesStepsContext
    {
        public Range[] Ranges { get; set; }
        public Range[] AggregatedRanges { get; set; }
    }

    [Binding]
    public class AggregateCoverageRangesSteps
    {
        private readonly AggregateCoverageRangesStepsContext _context;
        private readonly CoverageRangeSetsAggregator _rangeSetsAggregator;

        public AggregateCoverageRangesSteps(AggregateCoverageRangesStepsContext context)
        {
            _context = context;
            _rangeSetsAggregator = new CoverageRangeSetsAggregator();
        }

        [Given(@"I have following ranges")]
        public void GivenIHaveFollowingRanges(Table table)
        {
            _context.Ranges = table.CreateSet<Range>().ToArray();
        }

        [Then(@"the aggregation result should be")]
        public void ThenTheAggregationResultShouldBe(Table table)
        {
            var expected = table.CreateSet<Range>().ToList();

            var actual = _context.AggregatedRanges;

            actual.Should().BeEquivalentTo(expected);
        }

        [When(@"I aggregate ranges")]
        public void WhenIAggregateRanges()
        {
            _context.AggregatedRanges = _rangeSetsAggregator.AggregateRanges(_context.Ranges);
        }
    }
}