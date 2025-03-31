// MongoDB Playground
// Use Ctrl+Space inside a snippet or a string literal to trigger completions.

// The current database to use.
use("test");

db.getCollection("snapshots").aggregate([
    {
        $unwind: "$cowsOnFarm"
    },
    {
        $project: {
            _id: 0,
            tenantId: "$farmID",
            cowId: "$cowsOnFarm.cowId",
            date:  { $dateToString: { format: '%Y-%m-%d', date: '$sourceTs' } },
            data: "$cowsOnFarm",
        }
    },
    {
        $group: {
            _id: {
                _id: {
                    tenantId: "$tenantId",
                    cowId: "$cowId",
                    date: "$date",
                    data: "$data"
                }
          }
        }
    },
    {
        $project: {
            _id: 0,
            tenantId: "$_id._id.tenantId",
            cowId: "$_id._id.cowId",
            date: "$_id._id.date",
            data: "$_id._id.data",
        }
    },
    {
        $lookup: {
            from: "documents",
            let: {
                farmId: "$tenantId",
                cowId: "$cowId",
                date: "$date"
            },
            pipeline: [
                {
                    $match: {
                        $expr: {
                            $and: [
                                { $eq: ["$farmID", "$$farmId"] },
                                { $eq: ["$cowId", "$$cowId"] },
                                {
                                    $eq: [
                                        {
                                            $dateToString: {
                                                format: '%Y-%m-%d',
                                                date: "$startTs"
                                            }
                                        },
                                        "$$date"
                                    ]
                                }
                            ]
                        }
                    }
                }
            ],
          as: "yieldData"
        }
    },
    {
        $set: {
            yieldData: {
                $arrayElemAt: ["$yieldData", 0]
            }
        }
    },
    {
        $out: "entities"
    }
])