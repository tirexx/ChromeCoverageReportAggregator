Feature: Aggregate coverage ranges
	In order to get aggreagated coverage
	As an anybody
	I want to be able to calculate aggreagated coverage from multiple coverage range sets

@Ci
Scenario: Aggregate non-overlapping ranges
	Given I have following ranges
	| Start | End |
	| 1     | 10  |
	| 12    | 20  |
	| 25    | 30  |
	When I aggregate ranges
	Then the aggregation result should be
	| Start | End |
	| 1     | 10  |
	| 12    | 20  |
	| 25    | 30  |

@Ci
Scenario: Aggregate overlapping ranges
	Given I have following ranges
	| Start | End |
	| 1     | 10  |
	| 5     | 20  |
	| 25    | 30  |
	When I aggregate ranges
	Then the aggregation result should be
	| Start | End |
	| 1     | 20  |
	| 25    | 30  |

@Ci
Scenario: Aggregate inner ranges
	Given I have following ranges
	| Start | End |
	| 1     | 10  |
	| 3     | 7   |
	| 25    | 30  |
	When I aggregate ranges
	Then the aggregation result should be
	| Start | End |
	| 1     | 10  |
	| 25    | 30  |

@Ci
Scenario: Aggregate unordered ranges
	Given I have following ranges
	| Start | End |
	| 1     | 10  |
	| 31    | 35  |
	| 3     | 7   |
	| 8     | 30  |
	| 32    | 39  |
	When I aggregate ranges
	Then the aggregation result should be
	| Start | End |
	| 1     | 30  |
	| 31    | 39  |
