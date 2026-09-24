import { defineField, defineType } from 'sanity'
import { CubeIcon } from '@sanity/icons/Cube'

export const dimensionType = defineType({
  name: 'dimension',
  title: 'Dimension',
  type: 'document',
  icon: CubeIcon,

  fieldsets: [
    {
      name: 'identity',
      title: 'Identity'
    },
    {
      name: 'context',
      title: 'Context'
    },
    {
      name: 'content',
      title: 'Content'
    },
    {
      name: 'guidance',
      title: 'Guidance'
    },
    {
      name: 'introSteps',
      title: 'Intro Steps'
    },
    {
      name: 'publishing',
      title: 'Publishing'
    }
  ],

  fields: [

    // -------------------------
    // Identity
    // -------------------------

    defineField({
      name: 'title',
      title: 'Title',
      type: 'string',
      fieldset: 'identity',
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'slug',
      title: 'Slug',
      type: 'slug',
      fieldset: 'identity',
      options: {
        source: 'title',
        maxLength: 96
      },
      validation: Rule => Rule.required()
    }),

    defineField({
      name: 'icon',
      title: 'Icon',
      type: 'string',
      options: {
        list: [] // we will plug IconList later
      },
      fieldset: 'identity'
    }),

    // -------------------------
    // Context (WHERE it applies)
    // -------------------------

    defineField({
      name: 'milestones',
      title: 'Milestones',
      type: 'array',
      of: [
        {
          type: 'reference',
          to: [{ type: 'milestone' }]
        }
      ],
      fieldset: 'context',
      description: 'Milestones where this dimension applies'
    }),

    defineField({
      name: 'summary',
      title: 'Summary',
      type: 'text',
      rows: 3,
      fieldset: 'context'
    }),

    // -------------------------
    // Content (WHAT it is)
    // -------------------------

    defineField({
      name: 'purpose',
      title: 'Purpose',
      type: 'text',
      rows: 3,
      fieldset: 'content'
    }),

    defineField({
      name: 'biblicalFoundation',
      title: 'Biblical Foundation',
      type: 'array',
      of: [{ type: 'block' }],
      fieldset: 'content'
    }),

    // -------------------------
    // Guidance (HOW it helps)
    // -------------------------

    defineField({
      name: 'healthyMarkers',
      title: 'Healthy Markers',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'warningSigns',
      title: 'Warning Signs',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'growthFocus',
      title: 'Growth Focus',
      type: 'text',
      rows: 3,
      fieldset: 'guidance'
    }),

    // -------------------------
    // Publishing
    // -------------------------

    defineField({
      name: 'sortOrder',
      title: 'Sort Order',
      type: 'number',
      validation: Rule => Rule.required().min(1),
      fieldset: 'publishing'
    }),

    defineField({
      name: 'published',
      title: 'Published',
      type: 'boolean',
      initialValue: false,
      fieldset: 'publishing'
    })

  ],

  preview: {
    select: {
      title: 'title'
    }
  }
})