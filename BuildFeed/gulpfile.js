/// <binding BeforeBuild='sass-compile' ProjectOpened='watch-sass' />
var gulp = require("gulp");
var sass = require("gulp-sass");
var cleanCss = require("gulp-clean-css");
var sourceMaps = require("gulp-sourcemaps");

gulp.task("sass-compile",
    function ()
    {
        gulp.src("./res/css/*.scss")
            .pipe(sourceMaps.init())
            .pipe(sass())
            .pipe(cleanCss())
            .pipe(sourceMaps.write("./"))
            .pipe(gulp.dest("./res/css/"));
    });

gulp.task("watch-sass",
    function ()
    {
        gulp.watch("./res/scss/*.scss", ["sass-compile"]);
    });