/// <binding BeforeBuild='sass-compile' ProjectOpened='watch-sass' />
var gulp = require("gulp");
var sass = require("gulp-sass");
var cleanCss = require("gulp-clean-css");
var sourceMaps = require("gulp-sourcemaps");
var ts = require("gulp-typescript");
var uglify = require("gulp-uglify");
var autoprefixer = require("gulp-autoprefixer");

gulp.task("sass-compile",
    function ()
    {
        gulp.src("./res/css/*.scss")
            .pipe(sourceMaps.init())
            .pipe(sass())
            .pipe(autoprefixer({
                browsers: ["> 1% in GB", "IE 8"],
                cascade: false
            }))
            .pipe(cleanCss())
            .pipe(sourceMaps.write("./"))
            .pipe(gulp.dest("./res/css/"));
    });

gulp.task("typescript",
    function ()
    {
        var result = gulp.src("./res/ts/*.ts")
            .pipe(sourceMaps.init())
            .pipe(ts({
                target: "es5",
                sourceMap: false
            }));

        return result.js
            .pipe(uglify())
            .pipe(sourceMaps.write("./"))
            .pipe(gulp.dest("./res/ts/"));
    });

gulp.task("watch-sass",
    function ()
    {
        gulp.watch("./res/scss/*.scss", ["sass-compile"]);
        gulp.watch("./res/ts/*.ts", ["typescript"]);
    });