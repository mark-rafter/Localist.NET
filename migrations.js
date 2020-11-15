// remove obsolete PostDetail fields
db.PostDetail.update(
    {},
    {
        $unset: {
            Title: '',
            Author: '',
            Type: '',
            ExchangeDetails: '',
            ThumbUrl: ''
        }
    },
    { multi: true }
);

// add IsArchived to Posts
db.Post.update(
    {}, // selector
    { $set: { 'IsArchived': false } },
    false, // upsert
    true // multi
);

// convert Strings to ObjectId
db.Post.find({ 'Author': { $type: 3 } }).forEach(function (x) {
    x.Author.UserId = new ObjectId(x.Author.UserId);
    db.Post.save(x);
});

db.PostDetail.find({ 'Author': { $type: 3 } }).forEach(function (x) {
    x.Author.UserId = new ObjectId(x.Author.UserId);
    db.PostDetail.save(x);
});

db.PostDetail.find({ 'PostId': { $type: 2 } }).forEach(function (x) {
    x.PostId = new ObjectId(x.PostId);
    db.PostDetail.save(x);
});

db.PostReply.find({ 'Author': { $type: 3 } }).forEach(function (x) {
    x.Author.UserId = new ObjectId(x.Author.UserId);
    db.PostReply.save(x);
});

db.PostReply.find({ 'PostId': { $type: 2 } }).forEach(function (x) {
    x.PostId = new ObjectId(x.PostId);
    db.PostReply.save(x);
});

db.PostReply.find({ 'ParentId': { $type: 2 } }).forEach(function (x) {
    x.ParentId = new ObjectId(x.ParentId);
    db.PostReply.save(x);
});

db.Profile.find({ 'UserId': { $type: 2 } }).forEach(function (x) {
    x.UserId = new ObjectId(x.UserId);
    db.Profile.save(x);
});

// add invite code
db.Invite.insert({ CreatedOn: null, ModifiedOn: null, Code: 'bobross', Username: null });
