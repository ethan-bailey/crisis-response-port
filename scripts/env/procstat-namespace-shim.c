/*
 * Temporary runner workaround for .NET on hosts where getpid() reports the
 * container PID but /proc is mounted from a different PID namespace. In that
 * case /proc/<getpid()>/stat can be absent even though /proc/self/stat works.
 * Redirect only the current process's exact stat path; leave every other
 * process and procfs path untouched.
 *
 * Build with:
 *   gcc -shared -fPIC -O2 -Wall -Wextra -o /tmp/libprocshim.so \
 *     scripts/env/procstat-namespace-shim.c -ldl
 * Use with LD_PRELOAD=/tmp/libprocshim.so for the pinned dotnet CLI.
 */
#define _GNU_SOURCE
#include <dlfcn.h>
#include <fcntl.h>
#include <stdarg.h>
#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <unistd.h>

static const char *proc_stat_path(const char *path)
{
    static __thread char self_stat[64];
    char requested[64];

    if (path == NULL)
        return path;

    (void)snprintf(requested, sizeof(requested), "/proc/%d/stat", (int)getpid());
    if (strcmp(path, requested) != 0)
        return path;

    (void)strcpy(self_stat, "/proc/self/stat");
    return self_stat;
}

int open(const char *path, int flags, ...)
{
    static int (*next_open)(const char *, int, ...) = NULL;
    if (next_open == NULL)
        next_open = dlsym(RTLD_NEXT, "open");

    mode_t mode = 0;
    if ((flags & O_CREAT) != 0) {
        va_list args;
        va_start(args, flags);
        mode = (mode_t)va_arg(args, int);
        va_end(args);
    }

    path = proc_stat_path(path);
    return ((flags & O_CREAT) != 0)
        ? next_open(path, flags, mode)
        : next_open(path, flags);
}

int open64(const char *path, int flags, ...)
{
    static int (*next_open64)(const char *, int, ...) = NULL;
    if (next_open64 == NULL)
        next_open64 = dlsym(RTLD_NEXT, "open64");

    mode_t mode = 0;
    if ((flags & O_CREAT) != 0) {
        va_list args;
        va_start(args, flags);
        mode = (mode_t)va_arg(args, int);
        va_end(args);
    }

    path = proc_stat_path(path);
    return ((flags & O_CREAT) != 0)
        ? next_open64(path, flags, mode)
        : next_open64(path, flags);
}

int openat(int dirfd, const char *path, int flags, ...)
{
    static int (*next_openat)(int, const char *, int, ...) = NULL;
    if (next_openat == NULL)
        next_openat = dlsym(RTLD_NEXT, "openat");

    mode_t mode = 0;
    if ((flags & O_CREAT) != 0) {
        va_list args;
        va_start(args, flags);
        mode = (mode_t)va_arg(args, int);
        va_end(args);
    }

    path = proc_stat_path(path);
    return ((flags & O_CREAT) != 0)
        ? next_openat(dirfd, path, flags, mode)
        : next_openat(dirfd, path, flags);
}
