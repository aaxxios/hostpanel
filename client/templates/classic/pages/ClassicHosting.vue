<template>
  <UiStack gap="none" class="overflow-x-hidden" data-testid="classic-hosting">
    <!-- Hero -->
    <UiSection :contained="false" padding="lg" class="relative lg:py-32 bg-classic-page overflow-hidden">
      <UiStack gap="none" class="absolute inset-0 pointer-events-none">
        <UiStack gap="none" class="absolute top-0 right-1/4 w-[500px] h-[500px] bg-cyan-500/20 rounded-full blur-[140px] animate-blob" />
        <UiStack gap="none" class="absolute bottom-0 left-1/3 w-[400px] h-[400px] bg-primary-500/15 dark:bg-secondary-500/15 rounded-full blur-[130px] animate-blob animation-delay-2000" />
      </UiStack>

      <UiContainer class="relative z-10 text-center">
        <UiStack direction="row" align="center" gap="xs" class="inline-flex px-4 py-2 mb-6 rounded-full glass border border-cyan-500/20 backdrop-blur-sm">
          <Server :size="14" :stroke-width="2" class="text-cyan-700 dark:text-cyan-400" />
          <UiText as="span" size="xs" tone="muted" weight="medium" class="md:text-sm uppercase tracking-wider">{{ $t('hosting.badge') }}</UiText>
        </UiStack>

        <UiHeading :level="UiHeadingLevel.H1" size="title" class="md:text-6xl mb-6 leading-tight" data-testid="classic-hosting-title">
          {{ $t('hosting.title') }}
          <UiGradientText class="block pb-2 bg-gradient-to-r from-cyan-400 to-primary-400">{{ $t('hosting.titleHighlight') }}</UiGradientText>
        </UiHeading>
        <UiText tone="muted" class="md:text-lg mb-10 max-w-2xl mx-auto">
          {{ $t('hosting.subtitle') }}
        </UiText>
      </UiContainer>
    </UiSection>

    <!-- Plans -->
    <UiSection :contained="false" padding="md" class="relative md:py-20 bg-classic-page overflow-visible">
      <UiContainer class="relative z-10">
        <!-- Loading -->
        <UiGrid v-if="pending" cols="3" gap="lg" class="max-w-5xl mx-auto py-20 px-4" data-testid="classic-hosting-loading">
          <UiSkeleton v-for="i in 3" :key="i" shape="block" class="h-[500px] rounded-2xl" />
        </UiGrid>

        <!-- Error -->
        <UiStack v-else-if="error" align="center" gap="none" class="text-center py-20 px-4" data-testid="classic-hosting-error">
          <AlertCircle :size="48" :stroke-width="2" class="text-red-600 dark:text-red-400 mx-auto mb-4" />
          <UiText tone="muted">{{ $t('hosting.loadError') }}</UiText>
          <UiButton variant="primary" size="sm" class="mt-4" data-testid="classic-hosting-retry" @click="refresh()">{{ $t('common.retry') }}</UiButton>
        </UiStack>

        <template v-else>
          <!-- Billing cycle tabs -->
          <UiStack direction="row" justify="center" gap="none" class="sticky top-20 z-40 py-4 mb-16 md:mb-20 px-4">
            <UiSegmentedControl
              v-model="selectedCycle"
              :options="cycleOptions"
              :label="t('hosting.billingCycles')"
              size="sm"
              testid="classic-hosting-cycles"
              class="max-w-full overflow-x-auto no-scrollbar shadow-drop-bar backdrop-blur-xl"
            />
          </UiStack>

          <!-- Free hosting -->
          <UiStack v-if="freePlan" gap="none" class="max-w-6xl mx-auto mb-16" data-testid="classic-hosting-free">
            <UiCard
              v-motion
              padding="none"
              :initial="{ opacity: 0, scale: 0.95 }"
              :visible-once="{ opacity: 1, scale: 1, transition: { duration: 600 } }"
              class="relative overflow-hidden rounded-[2.5rem] border-cyan-500/30 dark:border-cyan-500/30 bg-gradient-to-br from-cyan-500/10 via-classic-surface to-primary-500/10 backdrop-blur-md p-8 md:p-12 shadow-halo-cyan-lg"
            >
              <UiStack align="center" justify="between" gap="xl" class="relative z-10 lg:flex-row">
                <UiStack gap="none" class="flex-1 text-center lg:text-left">
                  <UiStack direction="row" align="center" gap="xs" class="inline-flex self-center lg:self-start px-3 py-1 rounded-full bg-cyan-500/20 border border-cyan-500/30 mb-6">
                    <CheckCircle :size="14" class="text-cyan-700 dark:text-cyan-400" />
                    <UiText as="span" size="xs" tone="inherit" weight="bold" class="text-cyan-700 dark:text-cyan-400 md:text-xs font-black uppercase tracking-widest">{{ $t('hosting.getStartedForFree') }}</UiText>
                  </UiStack>
                  <UiHeading :level="UiHeadingLevel.H2" size="title" class="md:text-5xl font-black mb-4 uppercase tracking-tighter">
                    {{ formatPlanName(freePlan.name) }}
                  </UiHeading>
                  <!-- Interpolated, never `v-html`: the raw field is unsanitised operator HTML. -->
                  <UiText v-if="planSummary(freePlan)" weight="medium" class="md:text-lg mb-8 max-w-2xl mx-auto lg:mx-0 leading-relaxed text-gray-700 dark:text-gray-300">
                    {{ planSummary(freePlan) }}
                  </UiText>
                  <UiStack direction="row" wrap justify="center" gap="sm" class="lg:justify-start mb-4">
                    <UiStack
                      v-for="feature in planFeatures(freePlan).slice(0, 4)"
                      :key="feature"
                      direction="row"
                      align="center"
                      gap="xs"
                      class="text-xs md:text-sm text-gray-700 dark:text-gray-300 bg-white/[0.03] px-4 py-2.5 rounded-2xl border border-gray-200 dark:border-white/5 backdrop-blur-sm"
                    >
                      <Check :size="14" class="text-cyan-700 dark:text-cyan-400" />
                      {{ feature }}
                    </UiStack>
                  </UiStack>
                </UiStack>
                <UiStack align="center" gap="lg" class="w-full lg:w-[320px] lg:items-end text-center lg:text-right">
                  <UiStack gap="none">
                    <UiGradientText class="text-6xl font-black mb-1 uppercase tracking-tighter bg-gradient-to-b from-gray-900 to-gray-500 dark:from-white dark:to-gray-500">{{ $t('hosting.free') }}</UiGradientText>
                    <UiText size="xs" tone="subtle" weight="bold" class="uppercase tracking-widest">{{ $t('hosting.foreverFree') }}</UiText>
                  </UiStack>
                  <UiStack gap="sm" class="w-full">
                    <UiButton variant="primary" size="lg" full-width class="!rounded-2xl shadow-2xl shadow-cyan-500/20 !h-14 !text-base font-black uppercase tracking-widest" data-testid="classic-hosting-free-add" @click="addToCart(freePlan)">
                      <ShoppingCart :size="18" class="mr-2" />
                      {{ cart.hasItem(freePlan.pid, selectedCycle) ? $t('hosting.inCart') : $t('hosting.getStarted') }}
                    </UiButton>
                    <UiLink data-testid="classic-hosting-free-plan" :to="localePath(`/hosting/${planSlug(freePlan)}`)" tone="muted" :underline="false" class="text-xs font-bold hover:text-cyan-700 dark:hover:text-cyan-400 flex items-center justify-center gap-2 uppercase tracking-widest group">
                      {{ $t('hosting.viewAllFeatures') }}
                      <ArrowRight :size="14" class="group-hover:translate-x-1 transition-transform" />
                    </UiLink>
                  </UiStack>
                </UiStack>
              </UiStack>
            </UiCard>

            <!-- Separator -->
            <UiStack align="center" gap="none" class="text-center mt-24 mb-16">
              <UiText as="span" size="xs" tone="inherit" weight="bold" class="md:text-sm text-gray-500 dark:text-white/40 font-black uppercase tracking-[0.3em] block mb-4">{{ $t('hosting.premiumSolutions') }}</UiText>
              <UiStack gap="none" class="w-24 h-px bg-gradient-to-r from-transparent via-cyan-500 to-transparent mx-auto" />
            </UiStack>
          </UiStack>

          <!-- Other plans grid -->
          <UiStack direction="row" wrap justify="center" gap="lg" class="md:gap-8 mb-24" data-testid="classic-hosting-plans">
            <UiStack
              v-for="(plan, index) in otherPlans"
              :key="plan.pid"
              v-motion
              gap="none"
              :initial="{ opacity: 0, y: 30 }"
              :visible-once="{ opacity: 1, y: 0, transition: { delay: index * 100, duration: 500 } }"
              class="relative w-full sm:w-[calc(50%-12px)] lg:w-[calc(33.333%-22px)]"
              data-testid="classic-hosting-plan"
            >
              <UiCard
                padding="none"
                class="relative h-full p-8 rounded-[2.5rem] transition-all duration-500 flex flex-col overflow-hidden group/card bg-white/[0.01] border-gray-200 dark:border-white/5"
                :class="{ 'border-cyan-500/40 dark:border-cyan-500/40 bg-cyan-500/[0.03] shadow-halo-cyan-md': isFeatured(plan) }"
              >
                <UiStack v-if="isFeatured(plan)" gap="none" class="absolute -top-3 left-1/2 -translate-x-1/2 z-20">
                  <UiText as="span" size="xs" tone="inherit" weight="bold" class="px-5 py-1.5 bg-gradient-to-r from-cyan-500 to-primary-500 text-white text-[10px] font-black rounded-full shadow-xl shadow-cyan-500/30 uppercase tracking-widest">
                    {{ $t('hosting.mostPopular') }}
                  </UiText>
                </UiStack>

                <UiStack gap="none" class="mb-8">
                  <UiLink data-testid="classic-hosting-plan-link" :to="localePath(`/hosting/${planSlug(plan)}`)" tone="inherit" :underline="false" class="block mb-2">
                    <UiHeading :level="UiHeadingLevel.H2" size="title" class="md:text-3xl font-black hover:text-cyan-700 dark:hover:text-cyan-400 transition-colors uppercase tracking-tighter leading-none">
                      {{ formatPlanName(plan.name) }}
                    </UiHeading>
                  </UiLink>
                  <!--
                    No tagline line: `ProductDto` has no such field, so it never rendered.
                    The summary below is parsed out of `description` and interpolated, never
                    `v-html` - the raw field is unsanitised operator HTML.
                  -->
                  <UiText v-if="planSummary(plan)" size="sm" tone="subtle" class="leading-relaxed line-clamp-2 min-h-[40px] italic">
                    {{ planSummary(plan) }}
                  </UiText>
                </UiStack>

                <UiStack gap="none" class="mb-8 p-6 rounded-3xl bg-white/[0.03] border border-gray-200 dark:border-white/5">
                  <UiStack v-if="planPrice(plan, selectedCycle) !== null" direction="row" align="start" gap="none" class="mb-6 gap-1 items-baseline">
                    <UiText v-if="activeCurrencyPrefix" as="span" tone="subtle" weight="bold" class="text-xl">{{ activeCurrencyPrefix }}</UiText>
                    <UiText as="span" weight="bold" class="text-5xl font-black tabular-nums tracking-tighter" data-testid="classic-hosting-price">
                      {{ formatAmount(planPrice(plan, selectedCycle)) }}
                    </UiText>
                    <UiText v-if="payerCurrency.suffix" as="span" weight="bold" class="text-xl ml-1">{{ payerCurrency.suffix }}</UiText>
                    <UiText as="span" size="xs" tone="subtle" weight="bold" class="uppercase tracking-widest">/ {{ cycleLabel(selectedCycle) }}</UiText>
                  </UiStack>
                  <UiStack v-else gap="none" class="mb-6">
                    <UiText as="span" weight="bold" class="text-2xl font-black uppercase tracking-tighter">{{ $t('hosting.custom') }}</UiText>
                    <UiText as="span" size="xs" tone="subtle" class="text-[10px] italic mt-1">{{ $t('hosting.customContact') }}</UiText>
                  </UiStack>

                  <!-- Mini price list -->
                  <UiStack v-if="planCycles(plan).length > 1" gap="none" class="space-y-1.5 pt-4 border-t border-gray-200 dark:border-white/5">
                    <UiButton
                      v-for="c in planCycles(plan).slice(0, 4)"
                      :key="c.key"
                      variant="ghost"
                      size="sm"
                      full-width
                      class="justify-between text-[10px] px-3 py-2 rounded-xl duration-300"
                      :class="c.key === selectedCycle
                        ? 'bg-cyan-500/20 text-cyan-700 dark:text-cyan-300 font-black'
                        : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-300'"
                      :data-testid="`classic-hosting-mini-${c.key}`"
                      @click="selectedCycle = c.key"
                    >
                      <UiText as="span" tone="inherit" size="inherit" weight="medium" class="uppercase tracking-widest">{{ cycleLabel(c.key) }}</UiText>
                      <UiText as="span" tone="inherit" size="inherit" weight="medium" class="font-mono">{{ c.price }}</UiText>
                    </UiButton>
                  </UiStack>
                </UiStack>

                <UiList list-style="plain" class="space-y-4 mb-10 flex-1">
                  <UiListItem v-for="feature in planFeatures(plan)" :key="feature" class="flex items-start gap-3 text-sm text-gray-600 dark:text-gray-400 group-hover/card:text-gray-700 dark:group-hover/card:text-gray-300 transition-colors">
                    <UiStack direction="row" align="center" justify="center" gap="none" class="mt-1 w-5 h-5 rounded-full bg-cyan-500/10 border border-cyan-500/20 flex-shrink-0">
                      <CheckCircle :size="12" class="text-cyan-700 dark:text-cyan-400 flex-shrink-0" />
                    </UiStack>
                    {{ feature }}
                  </UiListItem>
                </UiList>

                <UiStack gap="sm">
                  <UiButton
                    variant="outline"
                    full-width
                    class="!rounded-2xl !h-12 font-black uppercase tracking-widest !text-xs transition-all shadow-xl hover:shadow-cyan-500/20"
                    :class="cart.hasItem(plan.pid, selectedCycle) ? '!bg-green-500/20 !text-green-700 dark:!text-green-400 !border-green-500/40' : '!border-gray-200 dark:!border-white/10 hover:!border-cyan-500/50'"
                    data-testid="classic-hosting-add"
                    @click="addToCart(plan)"
                  >
                    {{ cart.hasItem(plan.pid, selectedCycle) ? $t('hosting.inCart') : $t('hosting.addToCart') }}
                  </UiButton>
                  <UiButton
data-testid="classic-hosting-plan-select"
                    :to="localePath(`/hosting/${planSlug(plan)}`)"
                    variant="subtle"
                    full-width
                    class="!rounded-2xl py-3.5 text-[10px] font-black uppercase tracking-[0.2em]"
                  >
                    {{ $t('hosting.getStarted') }}
                  </UiButton>
                </UiStack>
              </UiCard>
            </UiStack>
          </UiStack>

          <!-- Agency section -->
          <UiStack v-if="agencyPlan" gap="none" class="max-w-6xl mx-auto mb-24" data-testid="classic-hosting-agency">
            <UiStack align="center" gap="none" class="text-center mb-16">
              <UiText as="span" size="xs" tone="inherit" weight="bold" class="md:text-sm text-gray-500 dark:text-white/40 font-black uppercase tracking-[0.3em] block mb-4">{{ $t('hosting.enterpriseSolutions') }}</UiText>
              <UiStack gap="none" class="w-24 h-px bg-gradient-to-r from-transparent via-primary-500 to-transparent mx-auto" />
            </UiStack>

            <UiCard
              v-motion
              padding="none"
              :initial="{ opacity: 0, y: 40 }"
              :visible-once="{ opacity: 1, y: 0, transition: { duration: 700 } }"
              class="relative rounded-3xl md:rounded-[3rem] border-primary-500/30 dark:border-primary-500/30 bg-gradient-to-br from-primary-500/10 via-classic-surface to-cyan-500/10 backdrop-blur-md p-8 md:p-12 shadow-halo-violet"
            >
              <UiStack align="center" justify="between" gap="xl" class="relative z-10 lg:flex-row">
                <UiStack gap="none" class="flex-1 text-center lg:text-left">
                  <UiStack direction="row" align="center" gap="xs" class="inline-flex self-center lg:self-start px-3 py-1 rounded-full bg-primary-500/20 border border-primary-500/30 mb-6">
                    <CheckCircle :size="14" class="text-primary-600 dark:text-primary-400" />
                    <UiText as="span" size="xs" tone="primary" weight="bold" class="md:text-xs font-black uppercase tracking-widest">{{ $t('hosting.agencyBadge') }}</UiText>
                  </UiStack>
                  <UiHeading :level="UiHeadingLevel.H2" size="title" class="md:text-6xl font-black mb-4 uppercase tracking-tighter leading-none">
                    {{ formatPlanName(agencyPlan.name) }}
                  </UiHeading>
                  <!-- Interpolated, never `v-html`: the raw field is unsanitised operator HTML. -->
                  <UiText v-if="planSummary(agencyPlan)" weight="medium" class="md:text-xl mb-8 max-w-2xl mx-auto lg:mx-0 leading-relaxed text-gray-700 dark:text-gray-300">
                    {{ planSummary(agencyPlan) }}
                  </UiText>
                  <UiStack direction="row" wrap justify="center" gap="sm" class="lg:justify-start">
                    <UiStack
                      v-for="feature in planFeatures(agencyPlan).slice(0, 6)"
                      :key="feature"
                      direction="row"
                      align="center"
                      gap="xs"
                      class="text-xs md:text-sm text-gray-700 dark:text-gray-300 bg-white/[0.03] px-4 py-2.5 rounded-2xl border border-gray-200 dark:border-white/5 backdrop-blur-sm"
                    >
                      <Check :size="14" class="text-primary-600 dark:text-primary-400" />
                      {{ feature }}
                    </UiStack>
                  </UiStack>
                </UiStack>
                <UiStack align="center" gap="xl" class="w-full lg:w-[320px] lg:items-end text-center lg:text-right">
                  <UiStack gap="none">
                    <UiStack v-if="planPrice(agencyPlan, selectedCycle) !== null" direction="row" align="start" justify="center" gap="none" class="lg:justify-end gap-1 items-baseline">
                      <UiText v-if="activeCurrencyPrefix" as="span" tone="subtle" weight="bold" class="text-2xl">{{ activeCurrencyPrefix }}</UiText>
                      <UiGradientText class="text-6xl md:text-7xl font-black tabular-nums tracking-tighter bg-gradient-to-b from-gray-900 to-primary-600 dark:from-white dark:to-primary-500 leading-none">
                        {{ formatAmount(planPrice(agencyPlan, selectedCycle)) }}
                      </UiGradientText>
                      <UiText v-if="payerCurrency.suffix" as="span" weight="bold" class="text-3xl ml-2">{{ payerCurrency.suffix }}</UiText>
                    </UiStack>
                    <UiText size="xs" tone="subtle" weight="bold" class="font-black uppercase tracking-[0.2em] mt-2">/ {{ cycleLabel(selectedCycle) }}</UiText>
                  </UiStack>
                  <UiStack gap="md" class="w-full">
                    <UiButton
                      variant="primary"
                      full-width
                      class="!rounded-2xl !h-16 !text-lg font-black uppercase tracking-widest shadow-2xl shadow-primary-500/30 transition-all scale-100 hover:scale-[1.02]"
                      data-testid="classic-hosting-agency-add"
                      @click="addToCart(agencyPlan)"
                    >
                      {{ cart.hasItem(agencyPlan.pid, selectedCycle) ? $t('hosting.inCart') : $t('hosting.addToCart') }}
                    </UiButton>
                    <UiLink data-testid="classic-hosting-agency-plan" :to="localePath(`/hosting/${planSlug(agencyPlan)}`)" tone="muted" :underline="false" class="text-xs font-black hover:text-primary-600 dark:hover:text-primary-400 flex items-center justify-center gap-3 uppercase tracking-[0.2em] group">
                      {{ $t('hosting.viewAllFeatures') }}
                      <ArrowRight :size="14" class="group-hover:translate-x-1.5 transition-transform" />
                    </UiLink>
                  </UiStack>
                </UiStack>
              </UiStack>
            </UiCard>
          </UiStack>

          <!-- Comparison table -->
          <UiStack id="compare-plans" gap="none" class="mt-32 overflow-hidden" data-testid="classic-hosting-compare">
            <UiStack align="center" gap="none" class="text-center mb-16">
              <UiHeading :level="UiHeadingLevel.H2" size="title" class="md:text-5xl font-black mb-4 uppercase tracking-tighter">{{ $t('hosting.comparison.title') }}</UiHeading>
              <UiText tone="muted" weight="medium" class="max-w-2xl mx-auto md:text-lg">{{ $t('hosting.comparison.subtitle') }}</UiText>
            </UiStack>

            <UiCard padding="none" class="relative rounded-3xl md:rounded-[2.5rem] bg-white/[0.01] overflow-hidden backdrop-blur-3xl group/table shadow-2xl">
              <UiStack gap="none" class="overflow-x-auto no-scrollbar scroll-smooth">
                <UiTable class="min-w-[320px] md:min-w-[1100px] border-0 rounded-none bg-transparent">
                  <UiTableHead>
                    <UiTableRow :hoverable="false" class="bg-gray-50 dark:bg-white/[0.03]">
                      <UiTableTh class="!p-4 md:!p-8 !text-[10px] md:!text-xs font-black sticky left-0 bg-classic-page z-30 min-w-[140px] md:min-w-[280px]">
                        <UiStack direction="row" align="center" gap="xs">
                          <Server :size="16" class="text-cyan-700 dark:text-cyan-400" />
                          {{ $t('hosting.comparison.feature') }}
                        </UiStack>
                      </UiTableTh>
                      <UiTableTh v-for="plan in plans" :key="plan.pid" align="center" class="!p-4 md:!p-8 relative group/col min-w-[120px]">
                        <UiStack gap="none" class="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-cyan-500 to-primary-500 scale-x-0 group-hover/col:scale-x-100 transition-transform duration-500" />
                        <UiText size="xs" weight="bold" class="md:text-sm mb-2 group-hover/col:text-cyan-700 dark:group-hover/col:text-cyan-400 transition-colors uppercase tracking-widest leading-tight">
                          {{ formatPlanName(plan.name) }}
                        </UiText>
                        <UiText v-if="planPrice(plan, selectedCycle) !== null" size="xs" tone="subtle" weight="bold" class="text-[8px] md:text-[10px] tracking-widest uppercase opacity-80 tabular-nums">
                          {{ formatMoney(planPrice(plan, selectedCycle), payerCurrency) }}
                        </UiText>
                      </UiTableTh>
                    </UiTableRow>
                  </UiTableHead>
                  <UiTableBody>
                    <!-- What each plan includes, exactly as the operator wrote it in the catalogue. -->
                    <UiTableRow group class="hover:bg-white/[0.03]">
                      <UiTableTd class="!p-4 md:!p-6 !text-[10px] md:!text-sm text-gray-600 dark:text-gray-400 font-bold sticky left-0 bg-classic-surface/95 backdrop-blur-xl z-20 border-r border-gray-200 dark:border-white/5 align-top group-hover:text-gray-900 dark:group-hover:text-white transition-colors">
                        <UiText as="span" tone="inherit" size="inherit" class="whitespace-nowrap">{{ $t('hosting.comparison.includes') }}</UiText>
                      </UiTableTd>
                      <UiTableTd v-for="plan in plans" :key="plan.pid" align="center" class="!p-4 md:!p-6 !text-[10px] md:!text-sm text-gray-500 align-top relative group-hover:text-gray-700 dark:group-hover:text-gray-200" data-testid="classic-hosting-compare-features">
                        <UiList v-if="planFeatures(plan).length" list-style="bare">
                          <UiListItem v-for="feature in planFeatures(plan)" :key="feature" :icon="Check">{{ feature }}</UiListItem>
                        </UiList>
                        <UiText v-else as="span" tone="inherit" class="text-gray-800 opacity-20 font-mono">{{ NO_VALUE }}</UiText>
                      </UiTableTd>
                    </UiTableRow>

                    <!-- Bottom action row -->
                    <UiTableRow :hoverable="false">
                      <UiTableTd class="!p-4 md:!p-8 sticky left-0 bg-classic-surface/95 backdrop-blur-xl z-20 border-r border-gray-200 dark:border-white/5" />
                      <UiTableTd v-for="plan in plans" :key="plan.pid" align="center" class="!p-4 md:!p-8 bg-white/[0.01]">
                        <UiButton
                          :variant="cart.hasItem(plan.pid, selectedCycle) ? 'subtle' : 'outline'"
                          size="sm"
                          class="px-4 md:px-6 text-[8px] md:text-xs font-black uppercase tracking-widest whitespace-nowrap hover:scale-110 active:scale-95"
                          :class="cart.hasItem(plan.pid, selectedCycle)
                            ? 'bg-green-500/20 border-green-500/40 text-green-600 dark:text-green-400'
                            : 'hover:border-cyan-500 hover:bg-cyan-500 hover:text-white'"
                          data-testid="classic-hosting-compare-add"
                          @click="addToCart(plan)"
                        >
                          {{ cart.hasItem(plan.pid, selectedCycle) ? $t('hosting.inCart') : $t('hosting.select') }}
                        </UiButton>
                      </UiTableTd>
                    </UiTableRow>
                  </UiTableBody>
                </UiTable>
              </UiStack>
              <!-- Mobile scroll info -->
              <UiStack direction="row" align="center" justify="center" gap="xs" class="md:hidden py-3 bg-white/[0.03] border-t border-gray-200 dark:border-white/5">
                <ArrowRight :size="10" class="animate-pulse" />
                <UiText as="span" size="xs" tone="subtle" weight="bold" class="text-[8px] uppercase tracking-[0.2em]">{{ $t('common.scrollHorizontal') }}</UiText>
              </UiStack>
            </UiCard>
          </UiStack>
        </template>
      </UiContainer>
    </UiSection>
  </UiStack>
</template>

<script setup lang="ts">
/**
 * ClassicHosting - the classic template's hosting page: hero, billing-cycle tabs, the free plan,
 * the plan grid, the agency plan and the comparison table, whose cells are each plan's own
 * catalogue feature lines.
 *
 * The catalogue is read through the catalogue store in the payer's own currency; prices are
 * formatted as sent, never converted. Cart actions go to the cart store.
 */
import { Server, CheckCircle, AlertCircle, ShoppingCart, Check, ArrowRight } from 'lucide-vue-next'
import { useCartStore } from '~/stores/cart'
import { useCurrencyStore } from '~/stores/currency'
import { useCatalogStore } from '~/stores/catalog'
import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import { PRICING_CYCLES, PRICING_CYCLE_I18N_KEY } from '~/constants/pricingCycle'
import { formatMoney, formatMoneyDigits } from '~/utils/formatMoney'
import { cycleAmount } from '~/utils/productFamilies'
import { parseDescription } from '~/utils/catalogGroups'
import { isFreePlan } from '~/utils/isFreePlan'
import { UiHeadingLevel } from '~/utils/ui/enums'
import type { HostingPlan } from '~/types/hostingplan'
import type { SegmentedOption } from '~/types/segmentedoption'
import { FeaturedFlag } from '~/types/featuredflag'
import { PricingCycle } from '~/types/pricingcycle'

/** Shown in a comparison cell for a plan that lists no features. */
const NO_VALUE = '—'

/** Fragment of a plan name that marks the agency plan; the catalogue has no flag for it. */
const AGENCY_NAME_MARKER = 'agency'

/**
 * Whether the operator flagged the plan as the featured one.
 *
 * @param plan - The plan.
 * @returns True for a featured plan.
 */
const isFeatured = (plan: HostingPlan): boolean => plan.is_featured === FeaturedFlag.On

/** Translate function and the active locale. */
const { t, locale } = useI18n()

/** Prefixes a path with the active locale. */
const localePath = useLocalePath()

/** Cart store: adds plans and tells which are already in. */
const cart = useCartStore()

/** Currency store: which currency the payer is billed in. */
const currencyStore = useCurrencyStore()

/** Catalogue store: the hosting plans. */
const catalogStore = useCatalogStore()

onMounted(() => {
  currencyStore.init()
  currencyStore.load()
  cart.init()
})

/** The payer's currency to format and price with - never chosen by the page's language. */
const payerCurrency = computed(() => currencyStore.moneyCurrencyFor(currencyStore.payerCode))

/** Currency symbol shown before an amount, when the currency puts it there. */
const activeCurrencyPrefix = computed(() => payerCurrency.value.prefix ?? '')

/** Filter that picks the hosting group out of the catalogue. */
const hostingFilter = { gid: HOSTING_GROUP_ID }

// The hosting plans are read in the payer's currency; the read runs again once `payerCode`
// resolves or changes, and `refresh` retries after a failure.
/** Whether the plans are loading, and the retry action. */
const { pending, refresh } = await useAsyncData('hosting:classic-plans', () => catalogStore.loadProducts(hostingFilter), {
  watch: [() => currencyStore.payerCode, locale]
})

/** The store's failure sentence, shown in place of the plans. */
const error = computed(() => catalogStore.error)

/** All hosting plans. */
const plans = computed<HostingPlan[]>(() => catalogStore.productsFor(hostingFilter) as HostingPlan[])

/**
 * Whether a plan is the agency one.
 *
 * @param plan - The plan.
 * @returns True when its name carries the agency marker.
 */
const isAgencyPlan = (plan: HostingPlan): boolean => plan.name.toLowerCase().includes(AGENCY_NAME_MARKER)

/** The free plan, shown in its own banner; free means a zero price, as in the catalogue store. */
const freePlan = computed(() => plans.value.find(p => isFreePlan(p.pricing)))

/** The agency plan, shown in its own banner. */
const agencyPlan = computed(() => plans.value.find(p => !isFreePlan(p.pricing) && isAgencyPlan(p)))

/** Every plan that is neither the free nor the agency one - the grid. */
const otherPlans = computed(() => plans.value.filter(p => !isFreePlan(p.pricing) && !isAgencyPlan(p)))

/** The billing cycle the prices are shown for. */
const selectedCycle = ref<PricingCycle>(PricingCycle.Monthly)

/**
 * Translated name of a billing cycle.
 *
 * @param cycle - The cycle.
 * @returns Its label.
 */
const cycleLabel = (cycle: PricingCycle): string =>
  t(`hosting.cycles.${PRICING_CYCLE_I18N_KEY[cycle]}`)

/**
 * The plan's price for one cycle, in the caller's own currency, as the API sent it.
 *
 * @param plan - The plan.
 * @param cycle - The billing cycle.
 * @returns The amount, or null when the plan carries no price for that cycle.
 */
const planPrice = (plan: HostingPlan, cycle: PricingCycle): number | null => cycleAmount(plan.pricing, cycle)

/** Cycles at least one plan is priced in - the segments of the cycle switch. */
const cycleOptions = computed<SegmentedOption<PricingCycle>[]>(() =>
  PRICING_CYCLES.filter(key => plans.value.some(p => planPrice(p, key) !== null))
    .map(key => ({ value: key, label: cycleLabel(key), testid: `classic-hosting-cycle-${key}` })))

/**
 * Formats just the grouped digits of an amount - no currency symbol - for markup that renders
 * the symbol separately at a different size. Decimal count comes from the payer's own
 * currency configuration, never a locale guess.
 *
 * @param amount - The amount, or null.
 * @returns The grouped digits, or '' for null.
 */
const formatAmount = (amount: number | null): string => {
  if (amount === null) {
    return ''
  }
  return formatMoneyDigits(amount, payerCurrency.value.decimals)
}

/**
 * The cycles a plan is priced in, each with its formatted price - the mini price list.
 *
 * @param plan - The plan.
 * @returns One entry per priced cycle.
 */
const planCycles = (plan: HostingPlan): Array<{ key: PricingCycle; price: string }> =>
  PRICING_CYCLES
    .filter(key => planPrice(plan, key) !== null)
    .map(key => ({ key, price: formatMoney(planPrice(plan, key), payerCurrency.value) }))

/**
 * Leading prose of a plan's description.
 *
 * `ProductDto` sends no `shortdescription`; the copy sits at the head of `description`.
 *
 * @param plan - The plan being rendered.
 * @returns The summary sentence, or `''` for a plan whose copy is bullets only.
 */
const planSummary = (plan: HostingPlan): string => parseDescription(plan.description || '').summary

/**
 * Feature lines of a plan's description.
 *
 * The copy is HTML, so `parseDescription` unwraps the tags, separates the leading prose and
 * strips the bullet markers.
 *
 * @param plan - The plan being rendered.
 * @returns The feature lines, markers stripped.
 */
const planFeatures = (plan: HostingPlan): string[] => parseDescription(plan.description || '').features

/**
 * Fixes the operator's misspelling of "business" in a plan name.
 *
 * @param name - The plan name.
 * @returns The corrected name.
 */
const formatPlanName = (name: string): string => {
  if (!name) {
    return ''
  }
  return name.replace(/Businnes/g, 'Business').replace(/businnes/g, 'business')
}

/**
 * The URL slug of a plan's detail page.
 *
 * @param plan - The plan.
 * @returns The lowercase, dash-separated name.
 */
const planSlug = (plan: HostingPlan): string =>
  formatPlanName(plan.name || '').toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '')

/**
 * Adds a plan to the cart at the selected cycle's price.
 *
 * @param plan - The plan to add; nothing happens without a price or a payer currency.
 */
const addToCart = (plan: HostingPlan): void => {
  const amount = planPrice(plan, selectedCycle.value)
  if (amount === null || !currencyStore.payerCode) {
    return
  }
  cart.addItem({
    pid: plan.pid || plan.id || 0,
    name: plan.name,
    billingcycle: selectedCycle.value,
    cycleLabel: cycleLabel(selectedCycle.value),
    amount,
    currency: currencyStore.payerCode
  })
}
</script>
