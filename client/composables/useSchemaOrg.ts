/**
 * Schema.org structured data composable
 * Provides JSON-LD markup for SEO
 *
 * @module composables/useSchemaOrg
 */

import type { MaybeRefOrGetter } from 'vue'
import { APP_LOCALES } from '~/constants/appLocales'
import { PortalSettingKey } from '~/constants/portalSettingKey'
import { SCHEMA_ORG_CONTEXT } from '~/constants/schemaOrg'

/** A service as the service schema describes it. */
interface SchemaService {
  /** Service name. */
  name: string
  /** Short description. */
  description: string
  /** Address of the service page, absolute or site-relative. */
  url: string
  /** Kind of service, e.g. `Web Hosting`. */
  serviceType: string
}

/** A software product as the product schema describes it. */
interface SchemaProduct {
  /** Product name. */
  name: string
  /** Short description. */
  description: string
  /** Address of the product page, absolute or site-relative. */
  url: string
  /** Product image. */
  image?: string
}

/** One purchasable plan of a product family, as an offer. */
interface SchemaPlanOffer {
  /** Plan name. */
  name: string
  /** Price of one billing period, in the currency below. */
  price: number
  /** ISO 4217 code of the currency the price is in. */
  priceCurrency: string
  /** ISO 8601 duration one payment covers, e.g. `P1M`. */
  billingDuration: string
  /** Address of the page the plan can be ordered from, absolute or site-relative. */
  url: string
}

/** A product family with an offer per plan. */
interface SchemaProductFamily {
  /** Family name. */
  name: string
  /** Short description. */
  description: string
  /** Address of the family page, absolute or site-relative. */
  url: string
  /** The plans on sale; a plan without a price is left out by the caller. */
  offers: SchemaPlanOffer[]
}

/** One step of a breadcrumb trail. */
interface SchemaBreadcrumb {
  /** Label of the step. */
  name: string
  /** Address of the step, absolute or site-relative. */
  url: string
}

/** One question and its answer. */
interface SchemaFaq {
  /** The question. */
  question: string
  /** The answer. */
  answer: string
}

/** A piece of writing (knowledge-base article, announcement) as the article schema describes it. */
interface SchemaArticle {
  /** Headline of the article. */
  headline: string
  /** Short plain-text summary. */
  description?: string
  /** Address of the article page, absolute or site-relative. */
  url: string
  /** ISO 8601 date the article was published, when the source gives one. */
  datePublished?: string
}

/**
 * Builds the JSON-LD documents of the public site and injects them into the head.
 *
 * @returns The base URL and one builder per schema type, plus the injector.
 */
export const useSchemaOrg = () => {
  const config = useRuntimeConfig()
  // The deployment's public address, as `useSeo` reads it; an unset one yields site-relative URLs rather than a made-up host.
  const baseUrl = config.public.baseUrl
  const { get } = usePortalSettings()
  // Everything about the operator comes from settings: name and tagline from site identity,
  // contact channels and address from `portal.contact.*`, the logo from `portal.logo`. A value
  // that is not set is omitted from the document instead of being published empty.
  const { name: siteName, tagline: siteTagline } = useSiteIdentity()
  const { email, phone, telegramUrl, whatsappUrl, address } = useContactDetails()

  /**
   * The operator's logo when uploaded, otherwise the bundled neutral one.
   *
   * @returns An absolute logo URL.
   */
  const logoUrl = (): string => {
    const uploaded = get(PortalSettingKey.Logo, 'portalLogo')
    if (!uploaded) {
      return `${baseUrl}/logo.png`
    }
    return uploaded.startsWith('http') ? uploaded : `${baseUrl}${uploaded}`
  }

  /** The postal address as structured data, or nothing when the operator set none. */
  const postalAddress = () => (address.value ? { address: { '@type': 'PostalAddress', streetAddress: address.value } } : {})

  /**
   * Organization schema.
   *
   * @returns The `Organization` JSON-LD object.
   */
  const organizationSchema = () => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'Organization',
      '@id': `${baseUrl}/#organization`,
      name: siteName.value,
      url: baseUrl,
      logo: {
        '@type': 'ImageObject',
        url: logoUrl()
      },
      ...(siteTagline.value ? { description: siteTagline.value } : {}),
      ...(phone.value ? { contactPoint: { '@type': 'ContactPoint', telephone: phone.value, contactType: 'customer service' } } : {}),
      ...postalAddress(),
      sameAs: [telegramUrl.value, whatsappUrl.value].filter(Boolean),
      ...(email.value ? { email: email.value } : {})
    }
  }

  /**
   * LocalBusiness schema.
   *
   * @returns The `LocalBusiness` JSON-LD object.
   */
  const localBusinessSchema = () => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'LocalBusiness',
      '@id': `${baseUrl}/#organization`,
      name: siteName.value,
      image: logoUrl(),
      url: baseUrl,
      ...(phone.value ? { telephone: phone.value } : {}),
      ...(email.value ? { email: email.value } : {}),
      ...postalAddress()
    }
  }

  /**
   * WebSite schema with search action.
   *
   * @returns The `WebSite` JSON-LD object.
   */
  const websiteSchema = () => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'WebSite',
      '@id': `${baseUrl}/#website`,
      url: baseUrl,
      name: siteName.value,
      ...(siteTagline.value ? { description: siteTagline.value } : {}),
      publisher: {
        '@id': `${baseUrl}/#organization`
      },
      inLanguage: [...APP_LOCALES]
    }
  }

  /**
   * Service schema. Names no served area: the operator's real coverage is not known here.
   *
   * @param service The service to describe.
   * @returns The `Service` JSON-LD object.
   */
  const serviceSchema = (service: SchemaService) => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'Service',
      name: service.name,
      description: service.description,
      url: service.url.startsWith('http') ? service.url : `${baseUrl}${service.url}`,
      serviceType: service.serviceType,
      provider: {
        '@id': `${baseUrl}/#organization`
      }
    }
  }

  /**
   * Product schema (for SaaS products). Carries no offer and no rating: neither is known
   * here, and a published figure that is not the operator's own is a false claim.
   *
   * @param product The product to describe.
   * @returns The `Product` JSON-LD object.
   */
  const productSchema = (product: SchemaProduct) => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'SoftwareApplication',
      name: product.name,
      description: product.description,
      url: product.url.startsWith('http') ? product.url : `${baseUrl}${product.url}`,
      applicationCategory: 'BusinessApplication',
      operatingSystem: 'Web',
      provider: {
        '@id': `${baseUrl}/#organization`
      }
    }
  }

  /**
   * Product schema for a product family: one `Offer` per plan, each priced for one billing period.
   * The figures are the catalogue's own, in the payer's currency; a family with no priced plan
   * publishes no `offers` at all rather than an empty list.
   *
   * @param family The product family and its plans.
   * @returns The `Product` JSON-LD object.
   */
  const productFamilySchema = (family: SchemaProductFamily) => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'Product',
      name: family.name,
      description: family.description,
      url: family.url.startsWith('http') ? family.url : `${baseUrl}${family.url}`,
      brand: {
        '@id': `${baseUrl}/#organization`
      },
      ...(family.offers.length > 0
        ? {
            offers: family.offers.map(offer => ({
              '@type': 'Offer',
              name: offer.name,
              url: offer.url.startsWith('http') ? offer.url : `${baseUrl}${offer.url}`,
              price: offer.price,
              priceCurrency: offer.priceCurrency,
              priceSpecification: {
                '@type': 'UnitPriceSpecification',
                price: offer.price,
                priceCurrency: offer.priceCurrency,
                billingDuration: offer.billingDuration
              }
            }))
          }
        : {})
    }
  }

  /**
   * BreadcrumbList schema.
   *
   * @param items The trail, from the home page down.
   * @returns The `BreadcrumbList` JSON-LD object.
   */
  const breadcrumbSchema = (items: SchemaBreadcrumb[]) => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'BreadcrumbList',
      itemListElement: items.map((item, index) => ({
        '@type': 'ListItem',
        position: index + 1,
        name: item.name,
        item: item.url.startsWith('http') ? item.url : `${baseUrl}${item.url}`
      }))
    }
  }

  /**
   * FAQ schema.
   *
   * @param faqs The questions and answers.
   * @returns The `FAQPage` JSON-LD object.
   */
  const faqSchema = (faqs: SchemaFaq[]) => {
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'FAQPage',
      mainEntity: faqs.map(faq => ({
        '@type': 'Question',
        name: faq.question,
        acceptedAnswer: {
          '@type': 'Answer',
          text: faq.answer
        }
      }))
    }
  }

  /**
   * Article schema.
   *
   * @param article The article; a relative `url` is made absolute.
   * @returns The `Article` JSON-LD object.
   */
  const articleSchema = (article: SchemaArticle) => {
    const url = article.url.startsWith('http') ? article.url : `${baseUrl}${article.url}`
    return {
      '@context': SCHEMA_ORG_CONTEXT,
      '@type': 'Article',
      headline: article.headline,
      mainEntityOfPage: url,
      url,
      publisher: { '@id': `${baseUrl}/#organization` },
      ...(article.description ? { description: article.description } : {}),
      ...(article.datePublished ? { datePublished: article.datePublished } : {})
    }
  }

  /**
   * Inject schema into head. A getter keeps the document current when the page's content or
   * language changes; it may return an empty list while the content is not there.
   *
   * @param schema One schema object or a list, as a value, ref or getter.
   */
  const injectSchema = (schema: MaybeRefOrGetter<object | object[]>) => {
    useHead(() => {
      const value = toValue(schema)
      const schemas = Array.isArray(value) ? value : [value]
      return {
        script: schemas.map(s => ({
          type: 'application/ld+json',
          // innerHTML, not `children`: the current unhead renders an unknown key as an
          // attribute, and the structured data would land in a `children` attribute of an
          // empty <script>, which no crawler reads.
          innerHTML: JSON.stringify(s)
        }))
      }
    })
  }

  return {
    baseUrl,
    organizationSchema,
    localBusinessSchema,
    websiteSchema,
    serviceSchema,
    productSchema,
    productFamilySchema,
    breadcrumbSchema,
    faqSchema,
    articleSchema,
    injectSchema
  }
}
