using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Raijin.CombinatoricsService.Infrastructure.Solvers.Cryptominisat;

internal enum PolarityMode { True, False, Rnd, Auto, Stable }

internal enum RestartStrategy { Auto, Geom, Glue, Luby }

internal enum SlsType { Walksat, Yalsat, Ccnr, CcnrYalsat }

/// <summary>
///     Fluent builder for CryptoMiniSat CLI arguments.
///     Only options that are explicitly set are emitted; the executable uses its own defaults for the rest.
/// </summary>
internal sealed class CryptominisatArgumentsBuilder
{
    private double? _adjustGlue;
    private bool? _allPreSimp;
    private string? _assump;
    private bool? _autoDisableGauss;
    private bool? _blockingGlue;
    private string? _branchStr;

    // BreakID
    private bool? _breakId;
    private int? _breakIdCls;
    private int? _breakIdEveryN;
    private bool? _breakIdMatrix;
    private int? _breakIdMaxCls;
    private int? _breakIdMaxLits;
    private int? _breakIdMaxVars;
    private int? _breakIdTime;

    // BVA
    private bool? _bva;
    private bool? _bva2Lit;
    private int? _bvaEveryN;
    private int? _bvaLim;
    private int? _bvaTo;
    private bool? _cardFind;
    private bool? _clearInter;
    private int? _confBtwSimp;
    private double? _confBtwSimpInc;
    private string? _debugLib;
    private bool? _decBased;
    private int? _diffDecLevelChrono;

    // Distillation
    private bool? _distill;
    private bool? _distillBin;
    private double? _distillIncConf;
    private double? _distillIrredAlsoRemRatio;
    private double? _distillIrredNoRemRatio;
    private int? _distillMaxM;
    private int? _distillMinConf;
    private int? _distillShuffleEveryN;
    private int? _distillSort;
    private double? _distillTier0Ratio;
    private double? _distillTier1Ratio;
    private string? _dumpResult;
    private bool? _emptyElim;
    private double? _eRatio;
    private int? _everyLev1;
    private int? _everyLev2;
    private int? _fullWatchConsEveryN;
    private int? _gateFindTo;
    private bool? _gates;
    private double? _gaussUsefulCutoff;
    private int? _glueCut0;
    private int? _glueCut1;
    private int? _glueHist;
    private bool? _idrup;
    private bool? _implicitManip;
    private int? _implStrTo;
    private int? _implSubsTo;
    private bool? _inTree;
    private int? _inTreeMaxM;
    private int? _lev1UseWithin;
    private double? _locgMult;
    private int? _lwrBndBlkRest;
    private long? _maxConfl;
    private int? _maxGlueHistLtLimited;
    private int? _maxMatrixCols;
    private int? _maxMatrixRows;
    private int? _maxNumMatrices;
    private int? _maxNumSimpPerSolve;

    // Gaussian elimination
    private int? _maxSccDepth;
    private int? _maxSol;
    private double? _maxTime;
    private int? _maxXorMat;
    private int? _maxXorSize;
    private double? _memOutMult;
    private int? _minMatrixRows;
    private bool? _moreMinim;
    private bool? _moreMoreAlways;
    private int? _moreMoreMinim;
    private double? _mult;
    private bool? _mustConsolidate;
    private bool? _mustRenumber;
    private double? _nextm;
    private bool _noBanSol;
    private bool? _nonStop;
    private int? _occIrredMaxMb;

    // Occurrence simplification
    private int? _occRedMax;
    private int? _occRedMaxMb;
    private bool? _occSimp;
    private bool? _otfHyper;

    // Polarity & branching
    private PolarityMode? _polar;
    private string? _preSchedule;
    private bool? _preSimp;
    private bool? _printGateDot;
    private bool? _printSol;
    private bool? _printTimes;
    private int? _random;
    private int? _ratioGlueGeom;

    // Minimization
    private bool? _recur;

    // Memory & renumbering
    private bool? _renumber;

    // Restart
    private RestartStrategy? _restart;
    private int? _restartPrint;
    private int? _rstFirst;
    private string? _sampling;
    private bool? _saveMem;

    // SCC & probing
    private bool? _scc;

    // Simplification
    private bool? _schedSimp;
    private string? _schedule;

    // SLS
    private bool? _sls;
    private int? _slsBumpType;
    private bool? _slsCcnrAspire;
    private int? _slsEveryN;
    private bool? _slsGetPhase;
    private int? _slsMaxMem;
    private int? _slsToBump;
    private int? _slsToBumpMaxPerVar;
    private SlsType? _slsType;
    private bool? _strengthen;

    // Misc
    private int? _strMaxT;
    private int? _strSTimeLim;
    private int? _subLongGoThrough;
    private int? _subsTimeLim;
    private double? _subsTimeLimBinRatio;
    private double? _subsTimeLimLongRatio;
    private int? _sync;

    // Ternary resolution
    private bool? _tern;
    private bool? _ternBinCreate;
    private double? _ternCreate;
    private int? _ternKeep;
    private int? _ternTimeLim;
    private int? _threads;
    private bool? _transRed;
    private bool? _updateGlueOnAnalysis;

    // Variable elimination
    private bool? _varElim;
    private bool? _varElimCheckRes;
    private int? _varElimMaxMb;
    private int? _varElimOver;

    private int? _varElimTo;

    // Core
    private int? _verb;
    private bool? _verbAllRestarts;
    private bool? _verbRestart;

    // Verbosity / output
    private int? _verbStat;
    private int? _walkSatRuns;
    private int? _weakenTimeLim;

    // XOR & gates
    private bool? _xor;
    private int? _xorFindTout;
    private int? _yalsatMems;

    // Output / proof
    private bool _zeroExitStatus;

    // ── Core ──────────────────────────────────────────────────────────────────

    /// <summary>Verbosity of solver. 0 = only solution. Range: 0–10.</summary>
    public CryptominisatArgumentsBuilder WithVerbosity(int level)
    {
        _verb = level;
        return this;
    }

    /// <summary>Stop solving after this many seconds.</summary>
    public CryptominisatArgumentsBuilder WithMaxTime(double seconds)
    {
        _maxTime = seconds;
        return this;
    }

    /// <summary>Stop solving after this many conflicts.</summary>
    public CryptominisatArgumentsBuilder WithMaxConflicts(long n)
    {
        _maxConfl = n;
        return this;
    }

    /// <summary>Random seed.</summary>
    public CryptominisatArgumentsBuilder WithRandomSeed(int seed)
    {
        _random = seed;
        return this;
    }

    /// <summary>Number of threads.</summary>
    public CryptominisatArgumentsBuilder WithThreads(int n)
    {
        _threads = n;
        return this;
    }

    /// <summary>Time multiplier for all simplification cutoffs.</summary>
    public CryptominisatArgumentsBuilder WithMultiplier(double m)
    {
        _mult = m;
        return this;
    }

    /// <summary>Global multiplier for when the next inprocessing should take place.</summary>
    public CryptominisatArgumentsBuilder WithNextMultiplier(double m)
    {
        _nextm = m;
        return this;
    }

    /// <summary>Multiplier for memory-out checks on inprocessing functions.</summary>
    public CryptominisatArgumentsBuilder WithMemOutMultiplier(double m)
    {
        _memOutMult = m;
        return this;
    }

    /// <summary>Search for this many solutions.</summary>
    public CryptominisatArgumentsBuilder WithMaxSolutions(int n)
    {
        _maxSol = n;
        return this;
    }

    /// <summary>Never stop the search() process.</summary>
    public CryptominisatArgumentsBuilder WithNonStop(bool enabled)
    {
        _nonStop = enabled;
        return this;
    }

    // ── Polarity & branching ─────────────────────────────────────────────────

    /// <summary>Selects polarity mode when branching.</summary>
    public CryptominisatArgumentsBuilder WithPolarity(PolarityMode mode)
    {
        _polar = mode;
        return this;
    }

    /// <summary>Branch strategy string, e.g. "vmtf+vsids".</summary>
    public CryptominisatArgumentsBuilder WithBranchStrategy(string strategy)
    {
        _branchStr = strategy;
        return this;
    }

    /// <summary>Don't ban the solution once it's found (flag, no value).</summary>
    public CryptominisatArgumentsBuilder WithNoBanSolution()
    {
        _noBanSol = true;
        return this;
    }

    // ── Restart ───────────────────────────────────────────────────────────────

    /// <summary>Restart strategy.</summary>
    public CryptominisatArgumentsBuilder WithRestartStrategy(RestartStrategy strategy)
    {
        _restart = strategy;
        return this;
    }

    /// <summary>The size of the base restart.</summary>
    public CryptominisatArgumentsBuilder WithRestartFirst(int n)
    {
        _rstFirst = n;
        return this;
    }

    /// <summary>Size of the moving window for short-term glue history.</summary>
    public CryptominisatArgumentsBuilder WithGlueHistory(int n)
    {
        _glueHist = n;
        return this;
    }

    /// <summary>Lower bound on blocking restart.</summary>
    public CryptominisatArgumentsBuilder WithLowerBoundBlockRestart(int n)
    {
        _lwrBndBlkRest = n;
        return this;
    }

    /// <summary>Multiplier for glue-based restart check.</summary>
    public CryptominisatArgumentsBuilder WithLocalGlueMultiplier(double m)
    {
        _locgMult = m;
        return this;
    }

    /// <summary>Ratio of glue vs geometric restarts.</summary>
    public CryptominisatArgumentsBuilder WithRatioGlueGeom(int ratio)
    {
        _ratioGlueGeom = ratio;
        return this;
    }

    /// <summary>Do blocking restart for glues.</summary>
    public CryptominisatArgumentsBuilder WithBlockingGlue(bool enabled)
    {
        _blockingGlue = enabled;
        return this;
    }

    /// <summary>Glue value for level-0 ('keep') cut.</summary>
    public CryptominisatArgumentsBuilder WithGlueCut0(int n)
    {
        _glueCut0 = n;
        return this;
    }

    /// <summary>Glue value for level-1 cut.</summary>
    public CryptominisatArgumentsBuilder WithGlueCut1(int n)
    {
        _glueCut1 = n;
        return this;
    }

    /// <summary>Lower the glue cutoff if more than this ratio of clauses is low glue.</summary>
    public CryptominisatArgumentsBuilder WithAdjustGlue(double ratio)
    {
        _adjustGlue = ratio;
        return this;
    }

    /// <summary>Reduce level-1 clauses every N conflicts.</summary>
    public CryptominisatArgumentsBuilder WithEveryLev1(int n)
    {
        _everyLev1 = n;
        return this;
    }

    /// <summary>Reduce level-2 clauses every N conflicts.</summary>
    public CryptominisatArgumentsBuilder WithEveryLev2(int n)
    {
        _everyLev2 = n;
        return this;
    }

    /// <summary>Learnt clause must be used in lev1 within this timeframe or be dropped.</summary>
    public CryptominisatArgumentsBuilder WithLev1UseWithin(int n)
    {
        _lev1UseWithin = n;
        return this;
    }

    // ── SCC & probing ────────────────────────────────────────────────────────

    /// <summary>Find equivalent literals through SCC and replace them.</summary>
    public CryptominisatArgumentsBuilder WithScc(bool enabled)
    {
        _scc = enabled;
        return this;
    }

    /// <summary>Remove useless binary clauses (transitive reduction).</summary>
    public CryptominisatArgumentsBuilder WithTransitiveReduction(bool enabled)
    {
        _transRed = enabled;
        return this;
    }

    /// <summary>Carry out intree-based probing.</summary>
    public CryptominisatArgumentsBuilder WithInTree(bool enabled)
    {
        _inTree = enabled;
        return this;
    }

    /// <summary>Time in mega-bogoprops to perform intree probing.</summary>
    public CryptominisatArgumentsBuilder WithInTreeMaxMega(int m)
    {
        _inTreeMaxM = m;
        return this;
    }

    /// <summary>Perform hyper-binary resolution during probing.</summary>
    public CryptominisatArgumentsBuilder WithOtfHyper(bool enabled)
    {
        _otfHyper = enabled;
        return this;
    }

    // ── Simplification ───────────────────────────────────────────────────────

    /// <summary>Perform simplification rounds.</summary>
    public CryptominisatArgumentsBuilder WithScheduledSimplification(bool enabled)
    {
        _schedSimp = enabled;
        return this;
    }

    /// <summary>Perform simplification at the very start.</summary>
    public CryptominisatArgumentsBuilder WithPreSimplification(bool enabled)
    {
        _preSimp = enabled;
        return this;
    }

    /// <summary>Perform simplification at every start (library mode only).</summary>
    public CryptominisatArgumentsBuilder WithAllPreSimplification(bool enabled)
    {
        _allPreSimp = enabled;
        return this;
    }

    /// <summary>Maximum number of simplifications per solve() call.</summary>
    public CryptominisatArgumentsBuilder WithMaxSimpPerSolve(int n)
    {
        _maxNumSimpPerSolve = n;
        return this;
    }

    /// <summary>Schedule for simplification during run.</summary>
    public CryptominisatArgumentsBuilder WithSchedule(string schedule)
    {
        _schedule = schedule;
        return this;
    }

    /// <summary>Schedule for simplification at startup.</summary>
    public CryptominisatArgumentsBuilder WithPreSchedule(string schedule)
    {
        _preSchedule = schedule;
        return this;
    }

    /// <summary>Perform occurrence-list-based optimisations.</summary>
    public CryptominisatArgumentsBuilder WithOccurrenceSimplification(bool enabled)
    {
        _occSimp = enabled;
        return this;
    }

    /// <summary>Start first simplification after this many conflicts.</summary>
    public CryptominisatArgumentsBuilder WithConflictsBetweenSimp(int n)
    {
        _confBtwSimp = n;
        return this;
    }

    /// <summary>Simplification rounds increment by this power.</summary>
    public CryptominisatArgumentsBuilder WithConflictsBetweenSimpInc(double d)
    {
        _confBtwSimpInc = d;
        return this;
    }

    // ── Ternary resolution ───────────────────────────────────────────────────

    /// <summary>Perform ternary resolution.</summary>
    public CryptominisatArgumentsBuilder WithTernary(bool enabled)
    {
        _tern = enabled;
        return this;
    }

    /// <summary>Time-out in bogoprops M of ternary resolution.</summary>
    public CryptominisatArgumentsBuilder WithTernaryTimeLimit(int m)
    {
        _ternTimeLim = m;
        return this;
    }

    /// <summary>Keep ternary resolution clauses only within this multiple of lev1usewithin.</summary>
    public CryptominisatArgumentsBuilder WithTernaryKeep(int n)
    {
        _ternKeep = n;
        return this;
    }

    /// <summary>Create only this multiple of ternary resolution clauses per simp run.</summary>
    public CryptominisatArgumentsBuilder WithTernaryCreate(double ratio)
    {
        _ternCreate = ratio;
        return this;
    }

    /// <summary>Allow ternary resolving to generate binary clauses.</summary>
    public CryptominisatArgumentsBuilder WithTernaryBinaryCreate(bool enabled)
    {
        _ternBinCreate = enabled;
        return this;
    }

    // ── Occurrence simplification ────────────────────────────────────────────

    /// <summary>Don't add to occur list any redundant clause larger than this.</summary>
    public CryptominisatArgumentsBuilder WithOccRedMax(int n)
    {
        _occRedMax = n;
        return this;
    }

    /// <summary>Don't allow redundant occur size beyond this many MB.</summary>
    public CryptominisatArgumentsBuilder WithOccRedMaxMb(int mb)
    {
        _occRedMaxMb = mb;
        return this;
    }

    /// <summary>Don't allow irredundant occur size beyond this many MB.</summary>
    public CryptominisatArgumentsBuilder WithOccIrredMaxMb(int mb)
    {
        _occIrredMaxMb = mb;
        return this;
    }

    /// <summary>Perform clause contraction through self-subsuming resolution.</summary>
    public CryptominisatArgumentsBuilder WithStrengthen(bool enabled)
    {
        _strengthen = enabled;
        return this;
    }

    /// <summary>Time-out in bogoprops M of weakening.</summary>
    public CryptominisatArgumentsBuilder WithWeakenTimeLimit(int m)
    {
        _weakenTimeLim = m;
        return this;
    }

    /// <summary>Time-out in bogoprops M of subsumption of long clauses.</summary>
    public CryptominisatArgumentsBuilder WithSubsTimeLimit(int m)
    {
        _subsTimeLim = m;
        return this;
    }

    /// <summary>Ratio of subsumption time limit for sub&amp;str long clauses with bin.</summary>
    public CryptominisatArgumentsBuilder WithSubsTimeLimitBinRatio(double ratio)
    {
        _subsTimeLimBinRatio = ratio;
        return this;
    }

    /// <summary>Ratio of subsumption time limit for sub long clauses with long.</summary>
    public CryptominisatArgumentsBuilder WithSubsTimeLimitLongRatio(double ratio)
    {
        _subsTimeLimLongRatio = ratio;
        return this;
    }

    /// <summary>Time-out in bogoprops M of strengthening of long clauses with long clauses.</summary>
    public CryptominisatArgumentsBuilder WithStrengtheningTimeLimit(int m)
    {
        _strSTimeLim = m;
        return this;
    }

    /// <summary>How many times to go through subsume.</summary>
    public CryptominisatArgumentsBuilder WithSubLongGoThrough(int n)
    {
        _subLongGoThrough = n;
        return this;
    }

    // ── BVA ──────────────────────────────────────────────────────────────────

    /// <summary>Perform bounded variable addition.</summary>
    public CryptominisatArgumentsBuilder WithBva(bool enabled)
    {
        _bva = enabled;
        return this;
    }

    /// <summary>Perform BVA only every N occ-simplify calls.</summary>
    public CryptominisatArgumentsBuilder WithBvaEveryN(int n)
    {
        _bvaEveryN = n;
        return this;
    }

    /// <summary>Maximum number of variables to add by BVA per call.</summary>
    public CryptominisatArgumentsBuilder WithBvaLimit(int n)
    {
        _bvaLim = n;
        return this;
    }

    /// <summary>BVA with 2-lit difference hack.</summary>
    public CryptominisatArgumentsBuilder WithBva2Lit(bool enabled)
    {
        _bva2Lit = enabled;
        return this;
    }

    /// <summary>BVA time limit in bogoprops M.</summary>
    public CryptominisatArgumentsBuilder WithBvaTimeOut(int m)
    {
        _bvaTo = m;
        return this;
    }

    // ── Variable elimination ──────────────────────────────────────────────────

    /// <summary>Perform variable elimination.</summary>
    public CryptominisatArgumentsBuilder WithVarElim(bool enabled)
    {
        _varElim = enabled;
        return this;
    }

    /// <summary>Var elimination bogoprops M time limit.</summary>
    public CryptominisatArgumentsBuilder WithVarElimTimeOut(int m)
    {
        _varElimTo = m;
        return this;
    }

    /// <summary>Do BVE until clause increase is less than X (power of 2).</summary>
    public CryptominisatArgumentsBuilder WithVarElimOver(int n)
    {
        _varElimOver = n;
        return this;
    }

    /// <summary>Perform empty resolvent elimination using bit-map trick.</summary>
    public CryptominisatArgumentsBuilder WithEmptyElim(bool enabled)
    {
        _emptyElim = enabled;
        return this;
    }

    /// <summary>Maximum extra MB of memory for new clauses during varelim.</summary>
    public CryptominisatArgumentsBuilder WithVarElimMaxMb(int mb)
    {
        _varElimMaxMb = mb;
        return this;
    }

    /// <summary>Eliminate at most this ratio of free variables per iteration.</summary>
    public CryptominisatArgumentsBuilder WithERatio(double ratio)
    {
        _eRatio = ratio;
        return this;
    }

    /// <summary>BVE should check whether resolvents subsume others.</summary>
    public CryptominisatArgumentsBuilder WithVarElimCheckRes(bool enabled)
    {
        _varElimCheckRes = enabled;
        return this;
    }

    // ── XOR & gates ──────────────────────────────────────────────────────────

    /// <summary>Discover long XORs.</summary>
    public CryptominisatArgumentsBuilder WithXor(bool enabled)
    {
        _xor = enabled;
        return this;
    }

    /// <summary>Maximum XOR size to find.</summary>
    public CryptominisatArgumentsBuilder WithMaxXorSize(int n)
    {
        _maxXorSize = n;
        return this;
    }

    /// <summary>Time limit for finding XORs.</summary>
    public CryptominisatArgumentsBuilder WithXorFindTimeout(int m)
    {
        _xorFindTout = m;
        return this;
    }

    /// <summary>Maximum matrix size to echelonize.</summary>
    public CryptominisatArgumentsBuilder WithMaxXorMatrix(int n)
    {
        _maxXorMat = n;
        return this;
    }

    /// <summary>Find gates.</summary>
    public CryptominisatArgumentsBuilder WithGates(bool enabled)
    {
        _gates = enabled;
        return this;
    }

    /// <summary>Print gate structure regularly to file 'gatesX.dot'.</summary>
    public CryptominisatArgumentsBuilder WithPrintGateDot(bool enabled)
    {
        _printGateDot = enabled;
        return this;
    }

    /// <summary>Max time in bogoprops M to find gates.</summary>
    public CryptominisatArgumentsBuilder WithGateFindTimeout(int m)
    {
        _gateFindTo = m;
        return this;
    }

    // ── Minimization ─────────────────────────────────────────────────────────

    /// <summary>Perform recursive minimisation.</summary>
    public CryptominisatArgumentsBuilder WithRecursiveMinimization(bool enabled)
    {
        _recur = enabled;
        return this;
    }

    /// <summary>Perform strong minimisation at conflict generation.</summary>
    public CryptominisatArgumentsBuilder WithMoreMinimization(bool enabled)
    {
        _moreMinim = enabled;
        return this;
    }

    /// <summary>Perform even stronger minimisation at conflict generation (level 0..2).</summary>
    public CryptominisatArgumentsBuilder WithMoreMoreMinimization(int level)
    {
        _moreMoreMinim = level;
        return this;
    }

    /// <summary>Always strong-minimise clause.</summary>
    public CryptominisatArgumentsBuilder WithMoreMoreAlways(bool enabled)
    {
        _moreMoreAlways = enabled;
        return this;
    }

    /// <summary>Create decision-based conflict clauses when UIP clause is too large.</summary>
    public CryptominisatArgumentsBuilder WithDecisionBased(bool enabled)
    {
        _decBased = enabled;
        return this;
    }

    /// <summary>Update glues while analyzing.</summary>
    public CryptominisatArgumentsBuilder WithUpdateGlueOnAnalysis(bool enabled)
    {
        _updateGlueOnAnalysis = enabled;
        return this;
    }

    /// <summary>Maximum glue used by glue-based restart strategy when populating history.</summary>
    public CryptominisatArgumentsBuilder WithMaxGlueHistLimited(int n)
    {
        _maxGlueHistLtLimited = n;
        return this;
    }

    /// <summary>Difference in decision level to trigger chronological backtracking. -1 = never.</summary>
    public CryptominisatArgumentsBuilder WithDiffDecLevelChrono(int n)
    {
        _diffDecLevelChrono = n;
        return this;
    }

    // ── SLS ──────────────────────────────────────────────────────────────────

    /// <summary>Run SLS during simplification.</summary>
    public CryptominisatArgumentsBuilder WithSls(bool enabled)
    {
        _sls = enabled;
        return this;
    }

    /// <summary>Which SLS algorithm to run.</summary>
    public CryptominisatArgumentsBuilder WithSlsType(SlsType type)
    {
        _slsType = type;
        return this;
    }

    /// <summary>Maximum MB to give to SLS solver.</summary>
    public CryptominisatArgumentsBuilder WithSlsMaxMemory(int mb)
    {
        _slsMaxMem = mb;
        return this;
    }

    /// <summary>Run SLS solver every N simplifications only.</summary>
    public CryptominisatArgumentsBuilder WithSlsEveryN(int n)
    {
        _slsEveryN = n;
        return this;
    }

    /// <summary>Run Yalsat with this many mems*million timeout.</summary>
    public CryptominisatArgumentsBuilder WithYalsatMems(int m)
    {
        _yalsatMems = m;
        return this;
    }

    /// <summary>Max 'runs' for WalkSAT.</summary>
    public CryptominisatArgumentsBuilder WithWalkSatRuns(int n)
    {
        _walkSatRuns = n;
        return this;
    }

    /// <summary>Get phase from SLS solver, set as new phase for CDCL.</summary>
    public CryptominisatArgumentsBuilder WithSlsGetPhase(bool enabled)
    {
        _slsGetPhase = enabled;
        return this;
    }

    /// <summary>Turn aspiration on/off for CCNR.</summary>
    public CryptominisatArgumentsBuilder WithSlsCcnrAspire(bool enabled)
    {
        _slsCcnrAspire = enabled;
        return this;
    }

    /// <summary>How many variables to bump in CCNR.</summary>
    public CryptominisatArgumentsBuilder WithSlsToBump(int n)
    {
        _slsToBump = n;
        return this;
    }

    /// <summary>How many times to bump an individual variable's activity in CCNR.</summary>
    public CryptominisatArgumentsBuilder WithSlsToBumpMaxPerVar(int n)
    {
        _slsToBumpMaxPerVar = n;
        return this;
    }

    /// <summary>How to calculate what variable to bump. 1=clause-based, 2=var-flip-based, 3=var-score-based.</summary>
    public CryptominisatArgumentsBuilder WithSlsBumpType(int type)
    {
        _slsBumpType = type;
        return this;
    }

    // ── BreakID ───────────────────────────────────────────────────────────────

    /// <summary>Run BreakID to break symmetries.</summary>
    public CryptominisatArgumentsBuilder WithBreakId(bool enabled)
    {
        _breakId = enabled;
        return this;
    }

    /// <summary>Run BreakID every N simplification iterations.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdEveryN(int n)
    {
        _breakIdEveryN = n;
        return this;
    }

    /// <summary>Maximum number of literals in thousands before BreakID won't run.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdMaxLiterals(int thousandsOfLiterals)
    {
        _breakIdMaxLits = thousandsOfLiterals;
        return this;
    }

    /// <summary>Maximum number of clauses in thousands before BreakID won't run.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdMaxClauses(int thousandsOfClauses)
    {
        _breakIdMaxCls = thousandsOfClauses;
        return this;
    }

    /// <summary>Maximum number of variables in thousands before BreakID won't run.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdMaxVars(int thousandsOfVars)
    {
        _breakIdMaxVars = thousandsOfVars;
        return this;
    }

    /// <summary>Maximum number of steps during automorphism finding.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdTime(int n)
    {
        _breakIdTime = n;
        return this;
    }

    /// <summary>Maximum number of breaking clauses per permutation.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdClauses(int n)
    {
        _breakIdCls = n;
        return this;
    }

    /// <summary>Detect matrix row interchangeability.</summary>
    public CryptominisatArgumentsBuilder WithBreakIdMatrix(bool enabled)
    {
        _breakIdMatrix = enabled;
        return this;
    }

    // ── Verbosity / output ────────────────────────────────────────────────────

    /// <summary>Verbosity of statistics at end of solving. Range: 0–3.</summary>
    public CryptominisatArgumentsBuilder WithVerbosityStatistics(int level)
    {
        _verbStat = level;
        return this;
    }

    /// <summary>Print more thorough restart stats.</summary>
    public CryptominisatArgumentsBuilder WithVerboseRestarts(bool enabled)
    {
        _verbRestart = enabled;
        return this;
    }

    /// <summary>Print a line for every restart.</summary>
    public CryptominisatArgumentsBuilder WithVerboseAllRestarts(bool enabled)
    {
        _verbAllRestarts = enabled;
        return this;
    }

    /// <summary>Print assignment if solution is SAT.</summary>
    public CryptominisatArgumentsBuilder WithPrintSolution(bool enabled)
    {
        _printSol = enabled;
        return this;
    }

    /// <summary>Print restart status lines at least every N conflicts.</summary>
    public CryptominisatArgumentsBuilder WithRestartPrint(int n)
    {
        _restartPrint = n;
        return this;
    }

    // ── Distillation ──────────────────────────────────────────────────────────

    /// <summary>Regularly execute clause distillation.</summary>
    public CryptominisatArgumentsBuilder WithDistill(bool enabled)
    {
        _distill = enabled;
        return this;
    }

    /// <summary>Regularly execute binary clause distillation.</summary>
    public CryptominisatArgumentsBuilder WithDistillBinary(bool enabled)
    {
        _distillBin = enabled;
        return this;
    }

    /// <summary>Maximum Mega-bogoprops to spend on distilling long clauses.</summary>
    public CryptominisatArgumentsBuilder WithDistillMaxMega(int m)
    {
        _distillMaxM = m;
        return this;
    }

    /// <summary>Multiplier for current conflicts OTF distill.</summary>
    public CryptominisatArgumentsBuilder WithDistillIncConfl(double ratio)
    {
        _distillIncConf = ratio;
        return this;
    }

    /// <summary>Minimum number of conflicts between OTF distill.</summary>
    public CryptominisatArgumentsBuilder WithDistillMinConfl(int n)
    {
        _distillMinConf = n;
        return this;
    }

    /// <summary>How much of tier 0 to distill.</summary>
    public CryptominisatArgumentsBuilder WithDistillTier0Ratio(double ratio)
    {
        _distillTier0Ratio = ratio;
        return this;
    }

    /// <summary>How much of tier 1 to distill.</summary>
    public CryptominisatArgumentsBuilder WithDistillTier1Ratio(double ratio)
    {
        _distillTier1Ratio = ratio;
        return this;
    }

    /// <summary>How much of irred to distill when doing also removal.</summary>
    public CryptominisatArgumentsBuilder WithDistillIrredAlsoRemRatio(double ratio)
    {
        _distillIrredAlsoRemRatio = ratio;
        return this;
    }

    /// <summary>How much of irred to distill when doing no removal.</summary>
    public CryptominisatArgumentsBuilder WithDistillIrredNoRemRatio(double ratio)
    {
        _distillIrredNoRemRatio = ratio;
        return this;
    }

    /// <summary>Shuffle to-be-distilled clauses every N cases randomly.</summary>
    public CryptominisatArgumentsBuilder WithDistillShuffleEveryN(int n)
    {
        _distillShuffleEveryN = n;
        return this;
    }

    /// <summary>Distill sorting type.</summary>
    public CryptominisatArgumentsBuilder WithDistillSort(int type)
    {
        _distillSort = type;
        return this;
    }

    // ── Memory & renumbering ──────────────────────────────────────────────────

    /// <summary>Renumber variables to increase CPU cache efficiency.</summary>
    public CryptominisatArgumentsBuilder WithRenumber(bool enabled)
    {
        _renumber = enabled;
        return this;
    }

    /// <summary>Always consolidate, even if not useful (debugging only).</summary>
    public CryptominisatArgumentsBuilder WithMustConsolidate(bool enabled)
    {
        _mustConsolidate = enabled;
        return this;
    }

    /// <summary>Save memory by deallocating variable space after renumbering.</summary>
    public CryptominisatArgumentsBuilder WithSaveMemory(bool enabled)
    {
        _saveMem = enabled;
        return this;
    }

    /// <summary>Treat all 'renumber' strategies as 'must-renumber'.</summary>
    public CryptominisatArgumentsBuilder WithMustRenumber(bool enabled)
    {
        _mustRenumber = enabled;
        return this;
    }

    /// <summary>Consolidate watchlists fully once every N conflicts.</summary>
    public CryptominisatArgumentsBuilder WithFullWatchConsEveryN(int n)
    {
        _fullWatchConsEveryN = n;
        return this;
    }

    // ── Misc ──────────────────────────────────────────────────────────────────

    /// <summary>Maximum MBP to spend on distilling long irred clauses through watches.</summary>
    public CryptominisatArgumentsBuilder WithStrMaxT(int m)
    {
        _strMaxT = m;
        return this;
    }

    /// <summary>Subsume and strengthen implicit clauses with each other.</summary>
    public CryptominisatArgumentsBuilder WithImplicitManip(bool enabled)
    {
        _implicitManip = enabled;
        return this;
    }

    /// <summary>Timeout (bogoprop Millions) of implicit subsumption.</summary>
    public CryptominisatArgumentsBuilder WithImplSubsTimeout(int m)
    {
        _implSubsTo = m;
        return this;
    }

    /// <summary>Timeout (bogoprop Millions) of implicit strengthening.</summary>
    public CryptominisatArgumentsBuilder WithImplStrTimeout(int m)
    {
        _implStrTo = m;
        return this;
    }

    /// <summary>Find cardinality constraints.</summary>
    public CryptominisatArgumentsBuilder WithCardFind(bool enabled)
    {
        _cardFind = enabled;
        return this;
    }

    /// <summary>Sync threads every N conflicts.</summary>
    public CryptominisatArgumentsBuilder WithSync(int n)
    {
        _sync = n;
        return this;
    }

    /// <summary>Interrupt threads cleanly, all the time.</summary>
    public CryptominisatArgumentsBuilder WithClearInter(bool enabled)
    {
        _clearInter = enabled;
        return this;
    }

    // ── Gaussian elimination ──────────────────────────────────────────────────

    /// <summary>The maximum for scc search depth.</summary>
    public CryptominisatArgumentsBuilder WithMaxSccDepth(int n)
    {
        _maxSccDepth = n;
        return this;
    }

    /// <summary>Maximum number of rows for gaussian matrix.</summary>
    public CryptominisatArgumentsBuilder WithMaxMatrixRows(int n)
    {
        _maxMatrixRows = n;
        return this;
    }

    /// <summary>Maximum number of columns for gaussian matrix.</summary>
    public CryptominisatArgumentsBuilder WithMaxMatrixCols(int n)
    {
        _maxMatrixCols = n;
        return this;
    }

    /// <summary>Automatically disable Gauss when performing badly.</summary>
    public CryptominisatArgumentsBuilder WithAutoDisableGauss(bool enabled)
    {
        _autoDisableGauss = enabled;
        return this;
    }

    /// <summary>Minimum number of rows for gaussian matrix.</summary>
    public CryptominisatArgumentsBuilder WithMinMatrixRows(int n)
    {
        _minMatrixRows = n;
        return this;
    }

    /// <summary>Maximum number of matrices to treat.</summary>
    public CryptominisatArgumentsBuilder WithMaxNumMatrices(int n)
    {
        _maxNumMatrices = n;
        return this;
    }

    /// <summary>Turn off Gauss if usefulness ratio is below this cutoff.</summary>
    public CryptominisatArgumentsBuilder WithGaussUsefulCutoff(double ratio)
    {
        _gaussUsefulCutoff = ratio;
        return this;
    }

    // ── Output / proof ────────────────────────────────────────────────────────

    /// <summary>Exit with status zero when solving finishes without issue (flag, no value).</summary>
    public CryptominisatArgumentsBuilder WithZeroExitStatus()
    {
        _zeroExitStatus = true;
        return this;
    }

    /// <summary>Print time it took for each simplification run.</summary>
    public CryptominisatArgumentsBuilder WithPrintTimes(bool enabled)
    {
        _printTimes = enabled;
        return this;
    }

    /// <summary>Enable idrup proof logging.</summary>
    public CryptominisatArgumentsBuilder WithIdrup(bool enabled)
    {
        _idrup = enabled;
        return this;
    }

    /// <summary>Set sampling vars, e.g. "1,84,44".</summary>
    public CryptominisatArgumentsBuilder WithSampling(string vars)
    {
        _sampling = vars;
        return this;
    }

    /// <summary>Assumptions file path.</summary>
    public CryptominisatArgumentsBuilder WithAssumptionsFile(string path)
    {
        _assump = path;
        return this;
    }

    /// <summary>Write solution(s) to this file.</summary>
    public CryptominisatArgumentsBuilder WithDumpResult(string path)
    {
        _dumpResult = path;
        return this;
    }

    /// <summary>Parse special comments to run solve/simplify during parsing of CNF.</summary>
    public CryptominisatArgumentsBuilder WithDebugLib(string value)
    {
        _debugLib = value;
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Builds the argument string containing only explicitly set options.
    ///     The CNF file path must be appended separately by the caller.
    /// </summary>
    internal string Build()
    {
        var sb = new StringBuilder();

        AppendValue(sb, "--verb", _verb);
        AppendValue(sb, "--maxtime", _maxTime);
        AppendValue(sb, "--maxconfl", _maxConfl);
        AppendValue(sb, "--random", _random);
        AppendValue(sb, "--threads", _threads);
        AppendValue(sb, "--mult", _mult);
        AppendValue(sb, "--nextm", _nextm);
        AppendValue(sb, "--memoutmult", _memOutMult);
        AppendValue(sb, "--maxsol", _maxSol);
        AppendBool01(sb, "--nonstop", _nonStop);

        AppendPolarityMode(sb);
        AppendValue(sb, "--branchstr", _branchStr);
        if (_noBanSol) AppendFlag(sb, "--nobansol");

        AppendRestartStrategy(sb);
        AppendValue(sb, "--rstfirst", _rstFirst);
        AppendValue(sb, "--gluehist", _glueHist);
        AppendValue(sb, "--lwrbndblkrest", _lwrBndBlkRest);
        AppendValue(sb, "--locgmult", _locgMult);
        AppendValue(sb, "--ratiogluegeom", _ratioGlueGeom);
        AppendBool01(sb, "--blockingglue", _blockingGlue);
        AppendValue(sb, "--gluecut0", _glueCut0);
        AppendValue(sb, "--gluecut1", _glueCut1);
        AppendValue(sb, "--adjustglue", _adjustGlue);
        AppendValue(sb, "--everylev1", _everyLev1);
        AppendValue(sb, "--everylev2", _everyLev2);
        AppendValue(sb, "--lev1usewithin", _lev1UseWithin);

        AppendBool01(sb, "--scc", _scc);
        AppendBool01(sb, "--transred", _transRed);
        AppendBool01(sb, "--intree", _inTree);
        AppendValue(sb, "--intreemaxm", _inTreeMaxM);
        AppendBool01(sb, "--otfhyper", _otfHyper);

        AppendBool01(sb, "--schedsimp", _schedSimp);
        AppendBool01(sb, "--presimp", _preSimp);
        AppendBool01(sb, "--allpresimp", _allPreSimp);
        AppendValue(sb, "--maxnumsimppersolve", _maxNumSimpPerSolve);
        AppendValue(sb, "--schedule", _schedule);
        AppendValue(sb, "--preschedule", _preSchedule);
        AppendBool01(sb, "--occsimp", _occSimp);
        AppendValue(sb, "--confbtwsimp", _confBtwSimp);
        AppendValue(sb, "--confbtwsimpinc", _confBtwSimpInc);

        AppendBoolTrueFalse(sb, "--tern", _tern);
        AppendValue(sb, "--terntimelim", _ternTimeLim);
        AppendValue(sb, "--ternkeep", _ternKeep);
        AppendValue(sb, "--terncreate", _ternCreate);
        AppendBool01(sb, "--ternbincreate", _ternBinCreate);

        AppendValue(sb, "--occredmax", _occRedMax);
        AppendValue(sb, "--occredmaxmb", _occRedMaxMb);
        AppendValue(sb, "--occirredmaxmb", _occIrredMaxMb);
        AppendBool01(sb, "--strengthen", _strengthen);
        AppendValue(sb, "--weakentimelim", _weakenTimeLim);
        AppendValue(sb, "--substimelim", _subsTimeLim);
        AppendValue(sb, "--substimelimbinratio", _subsTimeLimBinRatio);
        AppendValue(sb, "--substimelimlongratio", _subsTimeLimLongRatio);
        AppendValue(sb, "--strstimelim", _strSTimeLim);
        AppendValue(sb, "--sublonggothrough", _subLongGoThrough);

        AppendBool01(sb, "--bva", _bva);
        AppendValue(sb, "--bvaeveryn", _bvaEveryN);
        AppendValue(sb, "--bvalim", _bvaLim);
        AppendBool01(sb, "--bva2lit", _bva2Lit);
        AppendValue(sb, "--bvato", _bvaTo);

        AppendBool01(sb, "--varelim", _varElim);
        AppendValue(sb, "--varelimto", _varElimTo);
        AppendValue(sb, "--varelimover", _varElimOver);
        AppendBool01(sb, "--emptyelim", _emptyElim);
        AppendValue(sb, "--varelimmaxmb", _varElimMaxMb);
        AppendValue(sb, "--eratio", _eRatio);
        AppendBool01(sb, "--varelimcheckres", _varElimCheckRes);

        AppendBool01(sb, "--xor", _xor);
        AppendValue(sb, "--maxxorsize", _maxXorSize);
        AppendValue(sb, "--xorfindtout", _xorFindTout);
        AppendValue(sb, "--maxxormat", _maxXorMat);
        AppendBool01(sb, "--gates", _gates);
        AppendBool01(sb, "--printgatedot", _printGateDot);
        AppendValue(sb, "--gatefindto", _gateFindTo);

        AppendBool01(sb, "--recur", _recur);
        AppendBool01(sb, "--moreminim", _moreMinim);
        AppendValue(sb, "--moremoreminim", _moreMoreMinim);
        AppendBool01(sb, "--moremorealways", _moreMoreAlways);
        AppendBool01(sb, "--decbased", _decBased);
        AppendBool01(sb, "--updateglueonanalysis", _updateGlueOnAnalysis);
        AppendValue(sb, "--maxgluehistltlimited", _maxGlueHistLtLimited);
        AppendValue(sb, "--diffdeclevelchrono", _diffDecLevelChrono);

        AppendBool01(sb, "--sls", _sls);
        AppendSlsType(sb);
        AppendValue(sb, "--slsmaxmem", _slsMaxMem);
        AppendValue(sb, "--slseveryn", _slsEveryN);
        AppendValue(sb, "--yalsatmems", _yalsatMems);
        AppendValue(sb, "--walksatruns", _walkSatRuns);
        AppendBool01(sb, "--slsgetphase", _slsGetPhase);
        AppendBool01(sb, "--slsccnraspire", _slsCcnrAspire);
        AppendValue(sb, "--slstobump", _slsToBump);
        AppendValue(sb, "--slstobumpmaxpervar", _slsToBumpMaxPerVar);
        AppendValue(sb, "--slsbumptype", _slsBumpType);

        AppendBoolTrueFalse(sb, "--breakid", _breakId);
        AppendValue(sb, "--breakideveryn", _breakIdEveryN);
        AppendValue(sb, "--breakidmaxlits", _breakIdMaxLits);
        AppendValue(sb, "--breakidmaxcls", _breakIdMaxCls);
        AppendValue(sb, "--breakidmaxvars", _breakIdMaxVars);
        AppendValue(sb, "--breakidtime", _breakIdTime);
        AppendValue(sb, "--breakidcls", _breakIdCls);
        AppendBoolTrueFalse(sb, "--breakidmatrix", _breakIdMatrix);

        AppendValue(sb, "--verbstat", _verbStat);
        AppendBool01(sb, "--verbrestart", _verbRestart);
        AppendBool01(sb, "--verballrestarts", _verbAllRestarts);
        AppendBool01(sb, "--printsol", _printSol);
        AppendValue(sb, "--restartprint", _restartPrint);

        AppendBool01(sb, "--distill", _distill);
        AppendBool01(sb, "--distillbin", _distillBin);
        AppendValue(sb, "--distillmaxm", _distillMaxM);
        AppendValue(sb, "--distillincconf", _distillIncConf);
        AppendValue(sb, "--distillminconf", _distillMinConf);
        AppendValue(sb, "--distilltier0ratio", _distillTier0Ratio);
        AppendValue(sb, "--distilltier1ratio", _distillTier1Ratio);
        AppendValue(sb, "--distillirredalsoremratio", _distillIrredAlsoRemRatio);
        AppendValue(sb, "--distillirrednoremratio", _distillIrredNoRemRatio);
        AppendValue(sb, "--distillshuffleeveryn", _distillShuffleEveryN);
        AppendValue(sb, "--distillsort", _distillSort);

        AppendBool01(sb, "--renumber", _renumber);
        AppendBool01(sb, "--mustconsolidate", _mustConsolidate);
        AppendBool01(sb, "--savemem", _saveMem);
        AppendBool01(sb, "--mustrenumber", _mustRenumber);
        AppendValue(sb, "--fullwatchconseveryn", _fullWatchConsEveryN);

        AppendValue(sb, "--strmaxt", _strMaxT);
        AppendBool01(sb, "--implicitmanip", _implicitManip);
        AppendValue(sb, "--implsubsto", _implSubsTo);
        AppendValue(sb, "--implstrto", _implStrTo);
        AppendBool01(sb, "--cardfind", _cardFind);
        AppendValue(sb, "--sync", _sync);
        AppendBool01(sb, "--clearinter", _clearInter);

        AppendValue(sb, "--maxsccdepth", _maxSccDepth);
        AppendValue(sb, "--maxmatrixrows", _maxMatrixRows);
        AppendValue(sb, "--maxmatrixcols", _maxMatrixCols);
        AppendBoolTrueFalse(sb, "--autodisablegauss", _autoDisableGauss);
        AppendValue(sb, "--minmatrixrows", _minMatrixRows);
        AppendValue(sb, "--maxnummatrices", _maxNumMatrices);
        AppendValue(sb, "--gaussusefulcutoff", _gaussUsefulCutoff);

        if (_zeroExitStatus) AppendFlag(sb, "--zero-exit-status");
        AppendBool01(sb, "--printtimes", _printTimes);
        AppendBool01(sb, "--idrup", _idrup);
        AppendValue(sb, "--sampling", _sampling);
        AppendValue(sb, "--assump", _assump);
        AppendValue(sb, "--dumpresult", _dumpResult);
        AppendValue(sb, "--debuglib", _debugLib);

        return sb.ToString().TrimEnd();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void AppendFlag(StringBuilder sb, string flag)
        => sb.Append(flag).Append(' ');

    private static void AppendValue<T>(StringBuilder sb, string flag, T? value) where T : struct, IFormattable
    {
        if (value.HasValue)
            sb.Append(flag).Append(' ').Append(value.Value.ToString(null, CultureInfo.InvariantCulture)).Append(' ');
    }

    private static void AppendValue(StringBuilder sb, string flag, string? value)
    {
        if (value is not null)
            sb.Append(flag).Append(' ').Append(value).Append(' ');
    }

    /// <summary>Emits 1 for true, 0 for false — used by numeric boolean options.</summary>
    private static void AppendBool01(StringBuilder sb, string flag, bool? value)
    {
        if (value.HasValue)
            sb.Append(flag).Append(' ').Append(value.Value ? '1' : '0').Append(' ');
    }

    /// <summary>Emits "true" or "false" — used by options whose help shows {true,false}.</summary>
    private static void AppendBoolTrueFalse(StringBuilder sb, string flag, bool? value)
    {
        if (value.HasValue)
            sb.Append(flag).Append(' ').Append(value.Value ? "true" : "false").Append(' ');
    }

    private void AppendPolarityMode(StringBuilder sb)
    {
        if (_polar is null) return;
        string value = _polar.Value switch
        {
            PolarityMode.True => "true",
            PolarityMode.False => "false",
            PolarityMode.Rnd => "rnd",
            PolarityMode.Auto => "auto",
            PolarityMode.Stable => "stable",
            _ => throw new UnreachableException($"Unhandled {nameof(PolarityMode)}: {_polar.Value}")
        };
        sb.Append("--polar ").Append(value).Append(' ');
    }

    private void AppendRestartStrategy(StringBuilder sb)
    {
        if (_restart is null) return;
        string value = _restart.Value switch
        {
            RestartStrategy.Auto => "auto",
            RestartStrategy.Geom => "geom",
            RestartStrategy.Glue => "glue",
            RestartStrategy.Luby => "luby",
            _ => throw new UnreachableException($"Unhandled {nameof(RestartStrategy)}: {_restart.Value}")
        };
        sb.Append("--restart ").Append(value).Append(' ');
    }

    private void AppendSlsType(StringBuilder sb)
    {
        if (_slsType is null) return;
        string value = _slsType.Value switch
        {
            SlsType.Walksat => "walksat",
            SlsType.Yalsat => "yalsat",
            SlsType.Ccnr => "ccnr",
            SlsType.CcnrYalsat => "ccnr_yalsat",
            _ => throw new UnreachableException($"Unhandled {nameof(SlsType)}: {_slsType.Value}")
        };
        sb.Append("--slstype ").Append(value).Append(' ');
    }
}