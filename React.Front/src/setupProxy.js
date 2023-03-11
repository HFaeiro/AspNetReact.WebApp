const { createProxyMiddleware } = require('http-proxy-middleware');

const context = [
    "/api"
];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: 'https://localhost:7025',
        secure: false
    });

    app.use(appProxy);
    app.use(bodyParser.json({
        limit: '100000kb',
        type: 'application/json'
    }));
};
