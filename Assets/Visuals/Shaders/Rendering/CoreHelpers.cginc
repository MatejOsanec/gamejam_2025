#ifndef CORE_HELPERS_INCLUDED
#define CORE_HELPERS_INCLUDED

inline half GammaToLinearSpaceForAlpha (half a) {

    return a * (a * (a * 0.305306011h + 0.682171111h) + 0.012522878h);
}

#endif
