/**
 * Schema.org structured data composable
 * Provides JSON-LD markup for SEO
 *
 * @module composables/useSchemaOrg
 */

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

  /** The operator's logo when uploaded, otherwise the bundled neutral one. */
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
   * Organization schema
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
   * LocalBusiness schema
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
   * WebSite schema with search action
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
   * BreadcrumbList schema
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
   * FAQ schema
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
   * Inject schema into head
   */
  const injectSchema = (schema: object | object[]) => {
    const schemas = Array.isArray(schema) ? schema : [schema]

    useHead({
      script: schemas.map(s => ({
        type: 'application/ld+json',
        // innerHTML, not `children`: the current unhead renders an unknown key as an
        // attribute, and the structured data was landing in children="…" on an empty
        // <script> — emitted nowhere a crawler reads.
        innerHTML: JSON.stringify(s)
      }))
    })
  }

  return {
    baseUrl,
    organizationSchema,
    localBusinessSchema,
    websiteSchema,
    serviceSchema,
    productSchema,
    breadcrumbSchema,
    faqSchema,
    injectSchema
  }
}
