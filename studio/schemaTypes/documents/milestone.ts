import { defineField, defineType } from 'sanity'
import { BookIcon } from '@sanity/icons/Book'

import { Icons, IconList } from '../constants/icons'

export const milestoneType = defineType({
  name: 'milestone',
  title: 'Milestone',
  type: 'document',
  icon: BookIcon,

  fieldsets: [
    {
      name: 'identity',
      title: 'Identity'
    },
    {
      name: 'introduction',
      title: 'Introduction'
    },
    {
      name: 'biblicalFoundation',
      title: 'Biblical Foundation'
    },
    {
      name: 'guidance',
      title: 'Guidance'
    },
    {
      name: 'branding',
      title: 'Visual Identity'
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

    defineField({
      name: 'journey',
      title: 'Journey',
      type: 'reference',
      to: [{ type: 'journey' }],
      fieldset: 'identity',
      validation: Rule => Rule.required()
    }),

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
      name: 'sortOrder',
      title: 'Sort Order',
      type: 'number',
      fieldset: 'identity',
      validation: Rule =>
        Rule.required()
          .integer()
          .min(1)
    }),

    defineField({
      name: 'summary',
      title: 'Summary',
      type: 'text',
      rows: 3,
      fieldset: 'introduction'
    }),

    defineField({
      name: 'purpose',
      title: 'Purpose',
      type: 'text',
      rows: 4,
      fieldset: 'introduction'
    }),

    defineField({
      name: 'motto',
      title: 'Milestone Motto',
      type: 'string',
      fieldset: 'introduction'
    }),

    defineField({
      name: 'godsDesign',
      title: "God's Design",
      type: 'array',
      of: [{ type: 'block' }],
      fieldset: 'biblicalFoundation'
    }),

    defineField({
      name: 'healthyCharacteristics',
      title: 'Healthy Characteristics',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'potentialPitfalls',
      title: 'Potential Pitfalls',
      type: 'array',
      of: [{ type: 'string' }],
      fieldset: 'guidance'
    }),

    defineField({
      name: 'desiredOutcome',
      title: 'Desired Outcome',
      type: 'text',
      rows: 4,
      fieldset: 'guidance'
    }),

    defineField({
      name: 'icon',
      title: 'Icon',
      description: 'Icon representing this milestone throughout the application.',
      type: 'string',
      options: {
        list: IconList
      },
      fieldset: 'branding'
    }),

    defineField({
      name: 'heroImage',
      title: 'Hero Image',
      type: 'image',
      options: {
        hotspot: true
      },
      fieldset: 'branding'
    }),

    defineField({
  name: 'introPositiveOutlook',
  title: 'Intro 1 - Positive Outlook',
  description: 'A hopeful, encouraging introduction to this milestone.',
  type: 'text',
  rows: 4,
  fieldset: 'introSteps'
}),

defineField({
  name: 'introHelpsWith',
  title: 'Intro 2 - What This Milestone Helps With',
  description: 'Explain what this milestone will help the user reflect on or prepare for.',
  type: 'text',
  rows: 4,
  fieldset: 'introSteps'
}),

defineField({
  name: 'introBibleVerses',
  title: 'Intro 3 - Bible Verses',
  description: 'Bible verses connected to this milestone.',
  type: 'array',
  of: [
    {
      type: 'object',
      name: 'milestoneIntroBibleVerse',
      title: 'Bible Verse',
      fields: [
        defineField({
          name: 'reference',
          title: 'Reference',
          type: 'string',
          validation: Rule => Rule.required()
        }),
        defineField({
          name: 'text',
          title: 'Verse Text',
          type: 'text',
          rows: 3,
          validation: Rule => Rule.required()
        })
      ],
      preview: {
        select: {
          title: 'reference',
          subtitle: 'text'
        }
      }
    }
  ],
  fieldset: 'introSteps'
}),

defineField({
  name: 'introPrayerInvitation',
  title: 'Intro 4 - Prayer Invitation',
  description: 'Invite the user to pause and pray before continuing.',
  type: 'text',
  rows: 4,
  fieldset: 'introSteps'
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
      title: 'title',
      subtitle: 'journey.title',
      media: 'heroImage'
    }
  }
})