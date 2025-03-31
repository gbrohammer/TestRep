/* global use, db */
// MongoDB Playground
// To disable this template go to Settings | MongoDB | Use Default Template For Playground.
// Make sure you are connected to enable completions and to be able to run a playground.
// Use Ctrl+Space inside a snippet or a string literal to trigger completions.
// The result of the last command run in a playground is shown on the results panel.
// By default the first 20 documents will be returned with a cursor.
// Use 'console.log()' to print to the debug output.
// For more documentation on playgrounds please refer to
// https://www.mongodb.com/docs/mongodb-vscode/playgrounds/

use('test');
// Yield "per" cow per day
db.getCollection('entities').aggregate([
    {
        $match: {
            tenantId: 382
        }
    },
    {
        $group: {
            _id: {
                cowId: "$cowId",
                date: "$date"
            },
            yield: {
                $avg: "$yieldData.yield"
            }
        }
    },
    {
        $addFields: {
            cowId: "$_id.cowId",
            date: "$_id.date"
        },
    
    },
    // remove entries with null yield
    {
        $match: {
            yield: { $ne: null }
        }
    },
    {
        $project: {
            _id: 0,
        }
    }
])
    .toArray();


// Yield per cow per day
db.getCollection('entities').aggregate([
    {
        $match: {
            tenantId: 382
        }
    },
    {
        $group: {
            _id: {
                date: "$date"
            },
            yield: {
                $avg: "$yieldData.yield"
            }
        }
    },
    {
        $addFields: {
            date: "$_id.date"
        },
    
    },
    // remove entries with null yield
    {
        $match: {
            yield: { $ne: null }
        }
    },
    {
        $project: {
            _id: 0,
        }
    }
])
    .toArray();
